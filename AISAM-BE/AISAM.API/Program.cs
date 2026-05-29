using AISAM.API.Filters;
using AISAM.API.Middleware;
using AISAM.Common.Config;
using AISAM.Common.Models;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

ApplyEnvironmentOverride(builder.Configuration, "FRONTEND_BASE_URL", "FrontendSettings:BaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "JWT_SECRET_KEY", "JwtSettings:SecretKey");
ApplyEnvironmentOverride(builder.Configuration, "JWT_ISSUER", "JwtSettings:Issuer");
ApplyEnvironmentOverride(builder.Configuration, "JWT_AUDIENCE", "JwtSettings:Audience");
ApplyEnvironmentOverride(builder.Configuration, "GOOGLE_CLIENT_ID", "GoogleSettings:ClientId");
ApplyEnvironmentOverride(builder.Configuration, "GOOGLE_CLIENT_SECRET", "GoogleSettings:ClientSecret");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_HOST", "EmailSettings:SmtpHost");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_PORT", "EmailSettings:SmtpPort");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_USERNAME", "EmailSettings:SmtpUsername");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_PASSWORD", "EmailSettings:SmtpPassword");
ApplyEnvironmentOverride(builder.Configuration, "FROM_EMAIL", "EmailSettings:FromEmail");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<AisamContext>(options =>
        options.UseNpgsql(dataSource));
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("GoogleSettings"));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("FrontendSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", corsBuilder =>
    {
        corsBuilder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();

static void ApplyEnvironmentOverride(IConfiguration configuration, string environmentKey, string configurationKey)
{
    var value = Environment.GetEnvironmentVariable(environmentKey);
    if (!string.IsNullOrWhiteSpace(value))
    {
        configuration[configurationKey] = value;
    }
}
