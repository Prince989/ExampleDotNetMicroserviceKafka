using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using ProductService.Api.Middleware;
using ProductService.Application.Abstractions;
using ProductService.Application.Services;
using ProductService.Infrastructure.Database;
using ProductService.Infrastructure.Message;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var config = builder.Configuration;
var connectionString = config.GetConnectionString("Mongo");

var mongoClient = new MongoClient(connectionString);
var mongoDatabase = mongoClient.GetDatabase("ProductDB");

var jwtConfig = builder.Configuration.GetSection("Jwt");

var kafkaAddress = config.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";

builder.Services.AddSingleton(new KafkaProducer(kafkaAddress));

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


builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);

builder.Services.AddScoped<IRepository, MongoProductRepository>();

builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<UpdateProductHandler>();
builder.Services.AddScoped<FetchProductHandler>();
builder.Services.AddScoped<DeleteProductHandler>();

builder.Services.AddSingleton<IMessageProvider>(sp =>
    sp.GetRequiredService<KafkaProducer>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductService API", Version = "v1" });

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductService API v1");
    c.DisplayRequestDuration();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.ConfigObject.AdditionalItems["persistAuthorization"] = true; // keep token after refresh
});


app.MapControllers();

app.Run();