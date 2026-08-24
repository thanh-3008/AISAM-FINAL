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
using Microsoft.Extensions.Hosting;
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
if (!builder.Environment.IsEnvironment("Testing") && File.Exists(envPath))
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
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_FALLBACK_API_KEY_2", "GeminiSettings:FallbackApiKey2");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_FALLBACK_API_KEY_3", "GeminiSettings:FallbackApiKey3");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_FALLBACK_API_KEY_4", "GeminiSettings:FallbackApiKey4");
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

// === OpenAI (Primary Image + Video) ===
ApplyEnvironmentOverride(builder.Configuration, "OPENAI_API_KEY", "ImageProviderSettings:OpenAiApiKey");
ApplyEnvironmentOverride(builder.Configuration, "OPENAI_API_KEY", "VideoProviderSettings:OpenAiApiKey");
ApplyEnvironmentOverride(builder.Configuration, "OPENAI_IMAGE_MODEL", "ImageProviderSettings:OpenAiImageModel");
ApplyEnvironmentOverride(builder.Configuration, "OPENAI_IMAGE_QUALITY", "ImageProviderSettings:OpenAiImageQuality");
ApplyEnvironmentOverride(builder.Configuration, "OPENAI_VIDEO_MODEL", "VideoProviderSettings:OpenAiVideoModel");
ApplyEnvironmentOverride(builder.Configuration, "OPENAI_VIDEO_TIMEOUT_MINUTES", "VideoProviderSettings:OpenAiVideoTimeoutMinutes");

// === AI Image (DeAPI primary I2I + OpenRouter T2I fallback + HuggingFace fallback) ===
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_DEAPI_KEY", "ImageProviderSettings:DeApiApiKey");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_DEAPI_MODEL", "ImageProviderSettings:DeApiModel");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_KEY", "ImageProviderSettings:OpenRouterApiKey");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_MODEL", "ImageProviderSettings:OpenRouterModel");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_BASE_URL", "ImageProviderSettings:OpenRouterBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_EDIT_MODEL", "ImageProviderSettings:OpenRouterEditModel");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_EDIT_BASE_URL", "ImageProviderSettings:OpenRouterEditBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_EDIT_POLLING_INTERVAL_SECONDS", "ImageProviderSettings:OpenRouterEditPollingIntervalSeconds");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_OPENROUTER_EDIT_TIMEOUT_MINUTES", "ImageProviderSettings:OpenRouterEditTimeoutMinutes");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_HUGGINGFACE_KEY", "ImageProviderSettings:HuggingFaceApiKey");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_HUGGINGFACE_MODEL", "ImageProviderSettings:HuggingFaceModel");
ApplyEnvironmentOverride(builder.Configuration, "IMAGE_HUGGINGFACE_BASE_URL", "ImageProviderSettings:HuggingFaceBaseUrl");

// === AI Video (OpenRouter primary + DeAPI fallback + Colab) ===
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_ENABLED", "VideoProviderSettings:Enabled");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_GEMINI_KEY", "VideoProviderSettings:GeminiApiKey");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_GEMINI_MODEL", "VideoProviderSettings:GeminiModel");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_GEMINI_TIMEOUT", "VideoProviderSettings:GeminiTimeoutSeconds");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_KEY", "VideoProviderSettings:DeApiApiKey");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_KEY_FALLBACK", "VideoProviderSettings:DeApiApiKeyFallback");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_MODEL", "VideoProviderSettings:DeApiModel");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_MODEL_FALLBACK", "VideoProviderSettings:DeApiModelFallback");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_BASE_URL", "VideoProviderSettings:DeApiBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_IMG2VIDEO_MODEL", "VideoProviderSettings:DeApiImg2VideoModel");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_DEAPI_IMG2VIDEO_BASE_URL", "VideoProviderSettings:DeApiImg2VideoBaseUrl");

ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_BASE_URL", "VideoProviderSettings:ColabBaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_TOKEN", "VideoProviderSettings:ColabToken");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_TIMEOUT", "VideoProviderSettings:ColabTimeout");
ApplyEnvironmentOverride(builder.Configuration, "VIDEO_COLAB_FALLBACK_ENABLED", "VideoProviderSettings:EnableColabFallback");
ApplyEnvironmentOverride(builder.Configuration, "TAX_LOOKUP_ENDPOINT_TEMPLATE", "TaxLookup:EndpointTemplate");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var effectiveConnectionString = BuildDatabaseConnectionString(connectionString);
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(effectiveConnectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<AisamContext>(options =>
        options.UseNpgsql(dataSource, npgsqlOptions =>
            npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
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

var dataProtectionKeysPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_PATH");
if (string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, ".keys");
}

Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services
    .AddDataProtection()
    .SetApplicationName("AISAM.API")
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
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT Authentication Failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                if (context.AuthenticateFailure != null)
                {
                    logger.LogWarning("JWT Challenge Failed: {Message}", context.AuthenticateFailure.Message);
                }
                else
                {
                    logger.LogWarning("JWT Challenge Triggered without specific failure details (Token may be missing or invalid).");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("JWT Token Validated Successfully for user: {User}", context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

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
builder.Services.AddHttpClient<IProductImportService, ProductImportService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHttpClient<IBusinessKycService, BusinessKycService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<IOAuthStateStore>(_ => new SignedOAuthStateStore(jwtSecretKey));
builder.Services.AddScoped<ISocialTokenProtector, SocialTokenProtector>();
builder.Services.AddHttpClient<FacebookProvider>();
builder.Services.AddHttpClient<InstagramProvider>();
builder.Services.AddHttpClient<GoogleProvider>();
builder.Services.AddHttpClient<TikTokProvider>();
builder.Services.AddHttpClient<IPaymentService, PayOSPaymentService>();
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<FacebookProvider>());
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<InstagramProvider>());
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<GoogleProvider>());
builder.Services.AddHttpClient<GeminiTextClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<FallbackGeminiTextClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<FallbackGeminiTextClient2>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<FallbackGeminiTextClient3>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<FallbackGeminiTextClient4>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<OpenRouterTextClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddScoped<IGeminiTextClient, FallbackTextProvider>();
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<TikTokProvider>());
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IPromptEnhancerService, PromptEnhancerService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IContentScheduleService, ContentScheduleService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdCampaignService, AdCampaignService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddHttpClient<IHolidaySource, NagerDateHolidaySource>();
builder.Services.AddScoped<IWorkspaceDashboardService, WorkspaceDashboardService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IAutomationGenerationService, AutomationGenerationService>();
builder.Services.AddScoped<IAutomationApprovalService, AutomationApprovalService>();
builder.Services.AddScoped<IAutomationCreditService, AutomationCreditService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPostInsightsSyncService, PostInsightsSyncService>();
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
builder.Services.AddHostedService<CampaignInsightsBackgroundService>();
builder.Services.AddHostedService<PostInsightsBackgroundService>();

builder.Services.AddScoped<ICampaignInsightsSyncService, CampaignInsightsSyncService>();

builder.Services.Configure<ImageProviderSettings>(builder.Configuration.GetSection("ImageProviderSettings"));
builder.Services.Configure<VideoProviderSettings>(builder.Configuration.GetSection("VideoProviderSettings"));

// Clients (HttpClient)
builder.Services.AddHttpClient<OpenAIImageClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<OpenAIVideoClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<OpenRouterImageClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<HuggingFaceImageClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<GeminiVideoClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<DeApiVideoClient>(c => c.Timeout = TimeSpan.FromSeconds(60));

builder.Services.AddHttpClient<ColabVideoStrategy>();

// Providers
builder.Services.AddScoped<FallbackImageProvider>();
builder.Services.AddScoped<FallbackVideoProvider>();
builder.Services.AddScoped<IAIVideoProvider, FallbackVideoProvider>();
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
        var allowedOrigins = BuildAllowedOrigins(builder.Configuration);
        corsBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
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

app.UseResponseCompression();
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

static string BuildDatabaseConnectionString(string connectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    var configuredMaxPoolSize = builder.ContainsKey("Maximum Pool Size") || builder.ContainsKey("Max Pool Size");

    if (!configuredMaxPoolSize)
    {
        var maxPoolSize = 5;
        var envMaxPoolSize = Environment.GetEnvironmentVariable("DB_MAX_POOL_SIZE");
        if (int.TryParse(envMaxPoolSize, out var parsedMaxPoolSize) && parsedMaxPoolSize > 0)
        {
            maxPoolSize = parsedMaxPoolSize;
        }

        builder.MaxPoolSize = maxPoolSize;
    }

    if (!builder.ContainsKey("Timeout"))
    {
        builder.Timeout = 10;
    }

    if (!builder.ContainsKey("Command Timeout"))
    {
        builder.CommandTimeout = 30;
    }

    return builder.ConnectionString;
}

static string[] BuildAllowedOrigins(IConfiguration configuration)
{
    var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    void AddOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return;
        }

        var cleaned = origin.Trim().TrimEnd('/');
        if (Uri.TryCreate(cleaned, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            origins.Add(cleaned);
        }
    }

    foreach (var origin in configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
    {
        AddOrigin(origin);
    }

    AddOrigin(configuration["FrontendSettings:BaseUrl"]);

    var envOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
        ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");

    foreach (var origin in (envOrigins ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        AddOrigin(origin);
    }

    return origins.ToArray();
}


public partial class Program { }

