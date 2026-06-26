using AISAM.API.Filters;
using AISAM.API.Infrastructure;
using AISAM.API.Middleware;
using AISAM.Common.Config;
using AISAM.Common.Models;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_APP_ID", "FacebookSettings:AppId");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_APP_SECRET", "FacebookSettings:AppSecret");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_REDIRECT_URI", "FacebookSettings:RedirectUri");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_GRAPH_API_VERSION", "FacebookSettings:GraphApiVersion");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_BASE_URL", "FacebookSettings:BaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_OAUTH_URL", "FacebookSettings:OAuthUrl");
ApplyEnvironmentOverride(builder.Configuration, "GOOGLE_CLIENT_ID", "GoogleSettings:ClientId");
ApplyEnvironmentOverride(builder.Configuration, "GOOGLE_CLIENT_SECRET", "GoogleSettings:ClientSecret");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_HOST", "EmailSettings:SmtpHost");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_PORT", "EmailSettings:SmtpPort");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_USERNAME", "EmailSettings:SmtpUsername");
ApplyEnvironmentOverride(builder.Configuration, "SMTP_PASSWORD", "EmailSettings:SmtpPassword");
ApplyEnvironmentOverride(builder.Configuration, "FROM_EMAIL", "EmailSettings:FromEmail");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_API_KEY", "GeminiSettings:ApiKey");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_MODEL", "GeminiSettings:Model");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_MAX_TOKENS", "GeminiSettings:MaxTokens");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_TEMPERATURE", "GeminiSettings:Temperature");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_CLIENT_ID", "PayOSSettings:ClientId");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_API_KEY", "PayOSSettings:ApiKey");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_CHECKSUM_KEY", "PayOSSettings:ChecksumKey");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_BASE_URL", "PayOSSettings:BaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_RETURN_URL", "PayOSSettings:ReturnUrl");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_CANCEL_URL", "PayOSSettings:CancelUrl");
ApplyEnvironmentOverride(builder.Configuration, "CLOUDINARY_CLOUD_NAME", "CloudinarySettings:CloudName");
ApplyEnvironmentOverride(builder.Configuration, "CLOUDINARY_API_KEY", "CloudinarySettings:ApiKey");
ApplyEnvironmentOverride(builder.Configuration, "CLOUDINARY_API_SECRET", "CloudinarySettings:ApiSecret");

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
builder.Services.Configure<FacebookSettings>(builder.Configuration.GetSection("FacebookSettings"));
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("GoogleSettings"));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("FrontendSettings"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOSSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, ".keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecretKey = jwtSettings["SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured. Add JWT_SECRET_KEY to AISAM.API/.env.");
}

var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSigningKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(nameof(AISAM.Data.Enumeration.UserRoleEnum.Admin)));
});
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
builder.Services.AddScoped<IWorkspaceInvitationRepository, WorkspaceInvitationRepository>();
builder.Services.AddScoped<ICreditWalletRepository, CreditWalletRepository>();
builder.Services.AddScoped<ICreditUsageRecordRepository, CreditUsageRecordRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<IAiGenerationRepository, AiGenerationRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
builder.Services.AddScoped<ISocialIntegrationRepository, SocialIntegrationRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IContentCalendarRepository, ContentCalendarRepository>();
builder.Services.AddScoped<IPerformanceReportRepository, PerformanceReportRepository>();
builder.Services.AddScoped<IAdCampaignRepository, AdCampaignRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IWorkspaceInvitationService, WorkspaceInvitationService>();
builder.Services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();
builder.Services.AddScoped<ICreditService, CreditService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<IOAuthStateStore, MemoryOAuthStateStore>();
builder.Services.AddScoped<ISocialTokenProtector, SocialTokenProtector>();
builder.Services.AddHttpClient<FacebookProvider>();
builder.Services.AddHttpClient<GoogleProvider>();
builder.Services.AddHttpClient<IPaymentService, PayOSPaymentService>();
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<FacebookProvider>());
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<GoogleProvider>());
builder.Services.AddHttpClient<IGeminiTextClient, GeminiTextClient>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IContentScheduleService, ContentScheduleService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdCampaignService, AdCampaignService>();
builder.Services.AddScoped<IWorkspaceDashboardService, WorkspaceDashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IScheduledPostingService, ScheduledPostingService>();
builder.Services.AddScoped<IMediaStorageService, CloudinaryMediaStorageService>();
builder.Services.AddHostedService<ScheduledPostingBackgroundService>();

var controllers = builder.Services
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

controllers.ConfigureApplicationPartManager(manager =>
{
    var defaultProvider = manager.FeatureProviders.OfType<ControllerFeatureProvider>().FirstOrDefault();
    if (defaultProvider != null)
    {
        manager.FeatureProviders.Remove(defaultProvider);
    }

    manager.FeatureProviders.Add(new EnvironmentAwareControllerFeatureProvider(builder.Environment.IsDevelopment()));
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AISAM API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseMiddleware<ActiveProfileMiddleware>();
app.UseMiddleware<ActiveWorkspaceMiddleware>();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AISAM.Repositories.AisamContext>();
    var freeSubscriptions = dbContext.Subscriptions.Where(s => s.Plan == AISAM.Data.Enumeration.SubscriptionPlanEnum.Free).ToList();
    foreach (var sub in freeSubscriptions)
    {
        sub.Plan = AISAM.Data.Enumeration.SubscriptionPlanEnum.Premium;
        sub.QuotaPostsPerMonth = 20000;
        sub.QuotaAIContentPerDay = 1000;
        sub.QuotaAIImagesPerDay = 100;
        sub.QuotaPlatforms = 10;
        sub.QuotaAccounts = 10;
        sub.AnalysisLevel = 2;
        sub.QuotaAdBudgetMonthly = 10000000;
        sub.QuotaAdCampaigns = 100;
        sub.EndDate = DateTime.UtcNow.AddYears(1);
    }
    dbContext.SaveChanges();
}

app.Run();

static void ApplyEnvironmentOverride(IConfiguration configuration, string environmentKey, string configurationKey)
{
    var value = Environment.GetEnvironmentVariable(environmentKey);
    if (!string.IsNullOrWhiteSpace(value))
    {
        configuration[configurationKey] = value;
    }
}
