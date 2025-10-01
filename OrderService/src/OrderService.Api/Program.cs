using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OrderService.Application.Abstractions;
using OrderService.Application.Services;
using OrderService.Infrastructure.Database;
using OrderService.Infrastructure.Http;
using OrderService.Infrastructure.Message;
using OrderService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var config = builder.Configuration;

var jwtConfig = config.GetSection("Jwt");

var kafkaBootstrapServer = config["Kafka:BootstrapServers"] ?? "localhost:9092";

var connectionString = config.GetConnectionString("Mongo");

var mongoClient = new MongoClient(connectionString);

var database = mongoClient.GetDatabase("OrderDB");

builder.Services.AddHttpClient<IProductHttpClient, ProductHttpClient>(client =>
    client.BaseAddress = new Uri(config["ProductApiUrl"] ?? "http://product-service-svc.marketplace.svc.cluster.local:8002")
);

builder.Services.AddAuthorization();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!)),
            ValidateAudience = true,
            RoleClaimType = ClaimTypes.Role
        };
    }
);


builder.Services.AddSingleton(new KafkaProducer(kafkaBootstrapServer));

builder.Services.AddSingleton<IMessageProvider>(sp =>
    sp.GetRequiredService<KafkaProducer>()
);

builder.Services.AddSingleton<IMongoDatabase>(database);

builder.Services.AddScoped<IRepository, MongoRepository>();
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetOrdersHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService API", Version = "v1" });

    // 🔐 JWT Bearer definition
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token **_only_** (without the word Bearer).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);

    // Apply Bearer to all operations by default (you can still allow-anonymous per endpoint)
    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    };
    c.AddSecurityRequirement(securityRequirement);
});


var app = builder.Build();

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

app.Run();