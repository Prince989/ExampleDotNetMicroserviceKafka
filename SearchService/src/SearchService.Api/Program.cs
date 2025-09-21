using Microsoft.OpenApi.Models;
using Nest;
using SearchService.Api.Middleware;
using SearchService.Application.Abstractions;
using SearchService.Application.Services;
using SearchService.Domain.Entities;
using SearchService.Infrastructure.Cache;
using SearchService.Infrastructure.Elastic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var config = builder.Configuration;
var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092"; 

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

builder.Services.AddSingleton<ICacheRepository, CachedRepository>();

builder.Services.AddHostedService(sp => new KafkaConsumer(
    sp.GetRequiredService<ISearchRepository>(),
    kafkaBootstrap,
    sp.GetRequiredService<ILogger<KafkaConsumer>>()
));

builder.Services.AddSingleton<IElasticClient>(sp =>
{
    var elasticUri = config.GetValue<string>("Elastic:Uri") ?? "http://localhost:9200";
    var settings = new ConnectionSettings(new Uri(elasticUri))
        .DefaultMappingFor<ProductDocument>(m => m.IndexName("products").IdProperty(p => p.Id))
        .DefaultMappingFor<OrderDocument>(m => m.IndexName("orders").IdProperty(o => o.Id));
    return new ElasticClient(settings);
});

builder.Services.AddScoped<ISearchRepository, ElasticSearchRepository>();
builder.Services.AddScoped<SearchHandler>();

builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "SearchService API", Version = "v1" }));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var es     = scope.ServiceProvider.GetRequiredService<IElasticClient>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("ElasticBootstrap");
    await ElasticBootstrapper.BootstrapAsync(es, logger);
}

app.UseMiddleware<ExceptionMappingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService API v1");
    c.DisplayRequestDuration();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.ConfigObject.AdditionalItems["persistAuthorization"] = true; // keep token after refresh
});

app.MapControllers();

app.UseHttpsRedirection();

app.Run();