using _0_Framework.FileUploader;
using _0_Framework.Mappings.Profiles;
using AttendanceSystem.Infrastructure.ApiHelper;
using AttendanceSystem.Infrastructure.Bootstrapper;
using AuthenticationSystem.Infrastructure;
using HrSystem.Infrastructure.Bootstrapper;
using JavidanHR.WebHost.Utilities;
using JavidanHR.WebHost.Utilities.ReturnUrlFilter;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using PayrollSystem.Infrastructure.Bootstrapper;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Net;
using System.Security.Cryptography;
using System.Threading.RateLimiting;
using AttendanceSystem.Infrastructure.Context;
using HrSystem.Infrastructure.Context;
using PayrollSystem.Infrastructure.Persistence.Context;
using WebHost.Utilities;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

var builder = WebApplication.CreateBuilder(args);

//Add session 
builder.Services.AddSession();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/security.log",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Warning,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

/*
//configure redis cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
// 1. FusionCache with Redis backplane – fully resilient, no crash on startup
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromDays(14),                    // Normal cache duration
        IsFailSafeEnabled = true,                            // Critical: keeps old data if Redis is down
        FailSafeMaxDuration = TimeSpan.FromDays(30),         // Works up to 30 days without Redis
        FailSafeThrottleDuration = TimeSpan.FromSeconds(30)  // Prevents thundering herd
    })
    .WithStackExchangeRedisBackplane(options =>
    {
        options.Configuration = redisConnectionString;
        options.ConfigurationOptions ??= new ConfigurationOptions();
        options.ConfigurationOptions.AbortOnConnectFail = false;     // Never crash on startup
        options.ConfigurationOptions.ConnectRetry = 10;
        options.ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(2000);
    });

// 2. IDistributedCache with automatic fallback to in-memory (required for RedisTicketStore)
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = redisConnectionString;
    opt.InstanceName = "SCIP_";
}).AddDistributedMemoryCache();   // ← fallback when Redis is unavailable
*/


// بخش ۱: تنظیمات اولیه اتصال به Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
if (!redisConnectionString.Contains("connectTimeout"))
    redisConnectionString += ",connectTimeout=1000,abortConnect=false";

// بخش ۲: ثبت FusionCache
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromDays(14),
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromDays(30),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(30)
    });

// بخش ۳: تنظیم Backplane به صورت Lazy (اتصال فقط در صورت نیاز واقعی)
builder.Services.AddSingleton<Lazy<IFusionCacheBackplane>>(sp =>
{
    return new Lazy<IFusionCacheBackplane>(() =>
    {
        // نکته: نام کلاس صحیح RedisBackplane است
        var options = new RedisBackplaneOptions
        {
            Configuration = redisConnectionString
        };
        return new RedisBackplane(options);
    });
});


// 3. RedisService – simple, safe wrapper using IDistributedCache (fallback works automatically)
builder.Services.AddSingleton<RedisService>();

// 4. ITicketStore – automatically chooses Redis or Memory based on real connectivity
builder.Services.AddSingleton<ITicketStore>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RedisTicketStore>>();
    var distributedCache = sp.GetRequiredService<IDistributedCache>();
    var memoryCache = sp.GetRequiredService<IMemoryCache>();

    // این کلاس جدید با fallback داخلی (runtime) کار می‌کنه
    return new RedisTicketStore(
        distributedCache,
        memoryCache,
        logger,
        TimeSpan.FromDays(14)
    );
});

builder.Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ReturnUrlFilter>();
})
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddResponseCaching();

builder.Services.AddScoped<IFileUploadService, FileUploadService>();

builder.Services.AddScoped<AttendanceSystemApiHelper>();

//add string? returnUrl as injected parameter to each function 
builder.Services.AddScoped<IRequestContextAccessor, RequestContextAccessor>();

#region Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
#endregion

var connectionString = builder.Configuration.GetConnectionString("JavidanHR_DB");

#region Configure Bootstrapper classes
AuthenticationSystemBootstrapper.Configure(builder.Services, connectionString!);
HrSystemBootstrapper.Configure(builder.Services, connectionString!);
AttendanceSystemBootstrapper.Configure(builder.Services, connectionString!);
PayrollSystemBootstrapper.Configure(builder.Services, connectionString!);
#endregion

//AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(AppMappingProfile));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// add rate limiter 
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("DefaultPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB
    options.AllowResponseHeaderCompression = true;
    options.Limits.MaxRequestHeadersTotalSize = 65536;
});


// Persist Data Protection Keys
var keysPath = Path.Combine(Directory.GetCurrentDirectory(), "DataProtectionKeys");
if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
}

#pragma warning disable CA1416
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("JavidanHR")
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    })
    .ProtectKeysWithDpapi(protectToLocalMachine: true);
#pragma warning restore CA1416

var app = builder.Build();

// اتصال Backplane به FusionCache (غیراجباری، اما برای اطمینان)
var cache = app.Services.GetRequiredService<IFusionCache>();
var backplaneLazy = app.Services.GetRequiredService<Lazy<IFusionCacheBackplane>>();

// تنظیم Backplane روی نمونه FusionCache
if (cache is FusionCache fusionCache)
{
    fusionCache.SetupBackplane(backplaneLazy.Value);
}


//seed initial data and run migrations 
using (var scope = app.Services.CreateScope())
{
    var authContext = scope.ServiceProvider.GetRequiredService<AuthenticationSystemContext>();
    if (authContext.Database.GetPendingMigrations().Any())
    {
        authContext.Database.Migrate();
    }
    DbInitializer.Seed(authContext);

    var attContext = scope.ServiceProvider.GetRequiredService<AttendanceSystemContext>();
    if (attContext.Database.GetPendingMigrations().Any())
    {
        attContext.Database.Migrate();
    }

    var hrContext = scope.ServiceProvider.GetRequiredService<HrSystemContext>();
    if (hrContext.Database.GetPendingMigrations().Any())
    {
        hrContext.Database.Migrate();
    }

    var payrollContext = scope.ServiceProvider.GetRequiredService<PayrollSystemContext>();
    if (payrollContext.Database.GetPendingMigrations().Any())
    {
        payrollContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = (context) =>
    {
        var headers = context.Context.Response.GetTypedHeaders();
        headers.CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(365)
        };
    }
});

app.UseRateLimiter();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();


#region Exceptions and Errors MiddleWare

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Unhandled exception");

        context.Response.Clear();
        context.Response.StatusCode = 500;

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                status = 500,
                message = "خطای داخلی سرور"
            });
        }
        else
        {
            context.Request.Path = "/Error/Handle";
            context.Request.QueryString = new QueryString("?code=500");
            await next();
        }
    }
});


app.UseStatusCodePagesWithReExecute(
    "/Error/Handle",
    "?code={0}"
);

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    var request = context.HttpContext.Request;

    Log.Logger.Error($"{request.Path} | Error Code: {response.StatusCode}");

    if (request.Path.StartsWithSegments("/api"))
    {
        await response.WriteAsJsonAsync(new
        {
            success = false,
            status = response.StatusCode,
            message = "خطا در پردازش درخواست"
        });
    }
    else
    {
        request.Path = "/Error/Handle";
        request.QueryString = new QueryString($"?code={response.StatusCode}");
    }
});

#endregion



app.Use(async (context, next) =>
{
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    context.Items["ScriptNonce"] = nonce;

    var csp = new List<string>
    {
        "default-src 'self'",
        "script-src 'self' 'unsafe-inline' https://code.jquery.com https://cdn.jsdelivr.net https://cdnjs.cloudflare.com",
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
        "font-src 'self' https://fonts.gstatic.com data:",
        "img-src 'self' data: blob:",
        app.Environment.IsDevelopment()
            ? "connect-src 'self' http://localhost:* wss://localhost:*"
            : "connect-src 'self'",
        "frame-ancestors 'none'"
    };

    //context.Response.Headers.Append("Content-Security-Policy", string.Join("; ", csp));
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");
        return Task.CompletedTask;
    });
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    Log.Information("Starting web application");
    Log.Information(System.Net.ServicePointManager.SecurityProtocol.ToString());
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}