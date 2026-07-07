using Microsoft.AspNetCore.RateLimiting;
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

// The media endpoint accepts files up to 50 MB. Keep transport/form limits a
// little higher to account for multipart headers, otherwise Kestrel can reset
// the connection before the controller returns a useful validation response.
const long mediaRequestLimit = 55L * 1024 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = mediaRequestLimit);
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    options.MultipartBodyLengthLimit = mediaRequestLimit);

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
ApplyEnvironmentOverride(builder.Configuration, "INSTAGRAM_APP_ID", "InstagramSettings:AppId");
ApplyEnvironmentOverride(builder.Configuration, "INSTAGRAM_APP_SECRET", "InstagramSettings:AppSecret");
ApplyEnvironmentOverride(builder.Configuration, "INSTAGRAM_REDIRECT_URI", "InstagramSettings:RedirectUri");
ApplyEnvironmentOverride(builder.Configuration, "INSTAGRAM_GRAPH_API_VERSION", "InstagramSettings:GraphApiVersion");
ApplyEnvironmentOverride(builder.Configuration, "INSTAGRAM_BASE_URL", "InstagramSettings:BaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "INSTAGRAM_OAUTH_URL", "InstagramSettings:OAuthUrl");
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
ApplyEnvironmentOverride(builder.Configuration, "TEXT_OPENROUTER_KEY", "GeminiSettings:OpenRouterApiKey");
ApplyEnvironmentOverride(builder.Configuration, "TEXT_OPENROUTER_MODEL", "GeminiSettings:OpenRouterModel");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_FALLBACK_API_KEY", "GeminiSettings:FallbackApiKey");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_CLIENT_ID", "PayOSSettings:ClientId");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_API_KEY", "PayOSSettings:ApiKey");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_CHECKSUM_KEY", "PayOSSettings:ChecksumKey");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_BASE_URL", "PayOSSettings:BaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_RETURN_URL", "PayOSSettings:ReturnUrl");
ApplyEnvironmentOverride(builder.Configuration, "PAYOS_CANCEL_URL", "PayOSSettings:CancelUrl");
ApplyEnvironmentOverride(builder.Configuration, "CLOUDINARY_CLOUD_NAME", "CloudinarySettings:CloudName");
ApplyEnvironmentOverride(builder.Configuration, "CLOUDINARY_API_KEY", "CloudinarySettings:ApiKey");
ApplyEnvironmentOverride(builder.Configuration, "CLOUDINARY_API_SECRET", "CloudinarySettings:ApiSecret");
ApplyEnvironmentOverride(builder.Configuration, "TIKTOK_CLIENT_KEY", "TikTokSettings:ClientKey");
ApplyEnvironmentOverride(builder.Configuration, "TIKTOK_CLIENT_SECRET", "TikTokSettings:ClientSecret");
ApplyEnvironmentOverride(builder.Configuration, "TIKTOK_REDIRECT_URI", "TikTokSettings:RedirectUri");

// === AI Image (Gemini primary + OpenRouter fallback) ===
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_KEY", "ImageProviderSettings:OpenRouterApiKey");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_MODEL", "ImageProviderSettings:OpenRouterModel");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_BASE_URL", "ImageProviderSettings:OpenRouterBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_HUGGINGFACE_KEY", "ImageProviderSettings:HuggingFaceApiKey");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_HUGGINGFACE_MODEL", "ImageProviderSettings:HuggingFaceModel");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_HUGGINGFACE_BASE_URL", "ImageProviderSettings:HuggingFaceBaseUrl");

// === AI Video (OpenRouter primary + DeAPI fallback + Colab) ===
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_ENABLED", "VideoProviderSettings:Enabled");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_GEMINI_KEY", "VideoProviderSettings:GeminiApiKey");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_GEMINI_MODEL", "VideoProviderSettings:GeminiModel");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_GEMINI_TIMEOUT", "VideoProviderSettings:GeminiTimeoutSeconds");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_KEY", "VideoProviderSettings:DeApiApiKey");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_MODEL", "VideoProviderSettings:DeApiModel");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_BASE_URL", "VideoProviderSettings:DeApiBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_OPENROUTER_KEY", "VideoProviderSettings:OpenRouterApiKey");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_OPENROUTER_MODEL", "VideoProviderSettings:OpenRouterModel");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_OPENROUTER_BASE_URL", "VideoProviderSettings:OpenRouterBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_BASE_URL", "VideoProviderSettings:ColabBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_TOKEN", "VideoProviderSettings:ColabToken");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_TIMEOUT", "VideoProviderSettings:ColabTimeout");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_FALLBACK_ENABLED", "VideoProviderSettings:EnableColabFallback");

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
builder.Services.Configure<InstagramSettings>(builder.Configuration.GetSection("InstagramSettings"));
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("GoogleSettings"));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("FrontendSettings"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOSSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<TikTokSettings>(builder.Configuration.GetSection("TikTokSettings"));

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

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 3;
    });
});

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
builder.Services.AddScoped<IAutomationRepository, AutomationRepository>();
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
builder.Services.AddHttpClient<InstagramProvider>();
builder.Services.AddHttpClient<GoogleProvider>();
builder.Services.AddHttpClient<TikTokProvider>();
builder.Services.AddHttpClient<IPaymentService, PayOSPaymentService>();
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<FacebookProvider>());
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<InstagramProvider>());
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<GoogleProvider>());
builder.Services.AddHttpClient<GeminiTextClient>();
builder.Services.AddHttpClient<FallbackGeminiTextClient>();
builder.Services.AddHttpClient<OpenRouterTextClient>();
builder.Services.AddScoped<IGeminiTextClient, FallbackTextProvider>();
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<TikTokProvider>());
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IContentScheduleService, ContentScheduleService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdCampaignService, AdCampaignService>();
builder.Services.AddScoped<IWorkspaceDashboardService, WorkspaceDashboardService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IAutomationGenerationService, AutomationGenerationService>();
builder.Services.AddScoped<IAutomationApprovalService, AutomationApprovalService>();
builder.Services.AddScoped<IAutomationCreditService, AutomationCreditService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IScheduledPostingService, ScheduledPostingService>();
builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminSettingsService, AdminSettingsService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IMediaStorageService, CloudinaryMediaStorageService>();
builder.Services.AddSingleton<IBackgroundJobHealthService, BackgroundJobHealthService>();
builder.Services.AddHostedService<ScheduledPostingBackgroundService>();
builder.Services.AddHostedService<AutomationGenerationBackgroundService>();
builder.Services.AddHostedService<AutomationOperationsBackgroundService>();
builder.Services.AddHostedService<VideoPollingBackgroundService>();
builder.Services.AddHostedService<VideoGenerationBackgroundService>();

builder.Services.Configure<ImageProviderSettings>(builder.Configuration.GetSection("ImageProviderSettings"));
builder.Services.Configure<VideoProviderSettings>(builder.Configuration.GetSection("VideoProviderSettings"));

// Clients (HttpClient)
builder.Services.AddHttpClient<OpenRouterImageClient>();
builder.Services.AddHttpClient<HuggingFaceImageClient>();
builder.Services.AddHttpClient<GeminiVideoClient>();
builder.Services.AddHttpClient<DeApiVideoClient>();
builder.Services.AddHttpClient<OpenRouterVideoClient>();
builder.Services.AddHttpClient<ColabVideoStrategy>();

// Providers
builder.Services.AddScoped<FallbackImageProvider>();
builder.Services.AddScoped<FallbackVideoProvider>();
builder.Services.AddScoped<NullVideoProvider>();
builder.Services.AddScoped<ColabVideoStrategy>();
builder.Services.AddScoped<IVideoGenerationOrchestrator, VideoGenerationOrchestrator>();

// Factories
builder.Services.AddScoped<AIImageProviderFactory>();
builder.Services.AddScoped<AIVideoProviderFactory>();

// Interface -> Factory resolution
builder.Services.AddScoped<IAIImageProvider>(sp => sp.GetRequiredService<AIImageProviderFactory>().Create());
builder.Services.AddScoped<IAIVideoProvider>(sp => sp.GetRequiredService<AIVideoProviderFactory>().Create());

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
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        corsBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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
app.UseMiddleware<MaintenanceModeMiddleware>();

app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<ActiveProfileMiddleware>();
app.UseMiddleware<ActiveWorkspaceMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
if (app.Environment.IsDevelopment() && Environment.GetEnvironmentVariable("SEED_DEV_DATA") == "true")
{
    AISAM.API.Infrastructure.DevDataSeeder.SeedDevData(app.Services);
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
