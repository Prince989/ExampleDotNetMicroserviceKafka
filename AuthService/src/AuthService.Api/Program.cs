using System.Security.Claims;
using System.Text;
using AuthService.Application.Abstractions;
using AuthService.Application.Services;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using ProductService.Api.Middleware;
using IUserRepository = AuthService.Infrastructure.Interfaces.IUserRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var config =  builder.Configuration;

var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtConfig.Get<IJwtConfiguration>();

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

var mongoClient = new MongoClient(config.GetConnectionString("Mongo"));
var mongoDatabase = mongoClient.GetDatabase("UserDB");
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);

builder.Services.AddSingleton(jwtOptions!);

builder.Services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddScoped(typeof(IUserRepository), typeof(MongoUserRepository));
builder.Services.AddScoped<AuthHandler>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMappingMiddleware>();

app.UseAuthentication();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
