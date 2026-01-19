using System.Data.Common;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

using Api.Auth;
using Api.Infrastructure.Middleware;

using Infrastructure;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string corsPolicyName = "UiCors";

Environment.SetEnvironmentVariable("DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE", "false");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Why: Load secrets from the local user-secrets store (per developer machine),
    // so we never commit DB passwords / JWT signing keys into appsettings.*.json.
    // Note: CreateBuilder usually loads UserSecrets automatically when UserSecretsId exists,
    // but keeping this explicit prevents "it works on my machine" config drift.
    builder.Configuration.AddUserSecrets<Program>(true);
}

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Create ValidationProblemDetails (RFC 7807 style)
            var problemDetailsFactory = context.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>();

            var validationProblem = problemDetailsFactory.CreateValidationProblemDetails(
                context.HttpContext,
                context.ModelState,
                statusCode: StatusCodes.Status400BadRequest,
                title: "One or more validation errors occurred.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            );

            // Add traceId for easier debugging
            validationProblem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            validationProblem.Extensions["correlationId"] = CorrelationIdMiddleware.TryGet(context.HttpContext);
            // Ensure consistent instance (path)
            validationProblem.Instance = context.HttpContext.Request.Path;

            return new BadRequestObjectResult(validationProblem)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

// ============================================
// BL-015: Rate limiting for auth endpoints
// ============================================
builder.Services.AddRateLimiter(options =>
{
    // Return 429 when the limiter rejects the request
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Policy for login endpoint (per IP)
    options.AddPolicy("auth-login", httpContext =>
    {
        // Use forwarded headers if you run behind proxy; you already have UseForwardedHeaders in pipeline.
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                // Example: 10 requests per 1 minute per IP
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // no queue; reject immediately
            });
    });

    // Policy for register endpoint (per IP)
    options.AddPolicy("auth-register", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                // Example: 5 requests per 5 minutes per IP
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // Optional: one policy for "general auth" (refresh token, forgot password, etc.)
    options.AddPolicy("auth-general", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ip,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                // Example: 30 requests per 1 minute, smoother than fixed window
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6, // 10-second segments
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.OnRejected = async (context, ct) =>
    {
        // Return RFC7807 ProblemDetails for consistency
        context.HttpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests.",
            Detail = "Rate limit exceeded. Please retry later.",
            Instance = context.HttpContext.Request.Path
        };

        // Keep your existing traceId + correlationId patterns
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        problem.Extensions["correlationId"] =
            CorrelationIdMiddleware.TryGet(context.HttpContext);

        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken: ct);
    };
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["correlationId"] = CorrelationIdMiddleware.TryGet(ctx.HttpContext);
    };
});

// Bind + validate JWT options (strongly typed)
// - Issuer/Audience có default trong JwtOptions
// - Key là bắt buộc
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Missing Jwt:Key in configuration.")
    .Validate(o => o.Key.Length >= 32, "Jwt:Key must be at least 32 characters.")
    .ValidateOnStart();
builder.Services.AddSingleton<JwtTokenService>();

// Health checks
// - /health/live: process is up
// - /health/ready: app is ready to serve traffic (includes DB)
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<AppDbContext>("db");

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger doc basic
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CitizenshipApp API",
        Version = "v1"
    });

    // Bearer auth (JWT)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    c.AddSecurityRequirement(doc =>
    {
        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        );
        return requirement;
    });
});

// =====================
// CORS
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            string[] allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            // Safer default: if non-dev origins aren't configured, deny cross-origin requests
            // (instead of crashing or accidentally allowing any origin).
            if (allowedOrigins.Length == 0)
            {
                policy.SetIsOriginAllowed(_ => false)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                return;
            }

            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// DI Infrastructure (DbContext, repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Reverse proxy support (nginx / cloudflared / etc)
// Disabled by default for safety; enable via `Proxy:Enabled=true`.
if (builder.Configuration.GetValue<bool>("Proxy:Enabled"))
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = builder.Configuration.GetValue<int?>("Proxy:ForwardLimit") ?? 2;

        // When you enable this, the app is expected to run *behind* a trusted reverse proxy.
        // If you want stricter trust boundaries, configure KnownNetworks/KnownProxies.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Non-dev: avoid noisy warnings and make redirects deterministic behind a reverse proxy.
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = builder.Configuration.GetValue<int?>("HttpsRedirection:HttpsPort") ?? 443;
    });
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Read from configuration once at startup.
        // Defaults from JwtOptions (Issuer/Audience) are preserved when values are not provided.
        JwtOptions jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        byte[] keyBytes = Encoding.UTF8.GetBytes(jwt.Key);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await EnsureDatabaseReadyAsync(app, builder.Configuration);
}

// IMPORTANT: Forwarded headers must run very early in the pipeline.
if (builder.Configuration.GetValue<bool>("Proxy:Enabled"))
{
    app.UseForwardedHeaders();
}

// BL-014: correlation id + request logging (must run before exception handler)
app.UseMiddleware<CorrelationIdMiddleware>();


// Global exception handling -> ProblemDetails (RFC 7807)
// Why: standardize error responses and avoid leaking stack traces.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        IExceptionHandlerFeature? feature = context.Features.Get<IExceptionHandlerFeature>();
        Exception? ex = feature?.Error;
        if (ex is null)
        {
            return;
        }

        int statusCode = ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status404NotFound => "Not Found",
                _ => "Internal Server Error"
            },
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        problem.Extensions["correlationId"] = CorrelationIdMiddleware.TryGet(context);

        if (app.Environment.IsDevelopment())
        {
            problem.Detail = ex.Message;
            problem.Extensions["exceptionType"] = ex.GetType().FullName;
        }

        string json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await context.Response.WriteAsync(json);
    });
});

bool swaggerEnabled =
    app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CitizenshipApp API v1");
        options.RoutePrefix = "swagger";
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Name == "self"
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Name == "self" || r.Name == "db"
});

if (app.Environment.IsDevelopment())
{
    // Convenience for local dev: opening http://localhost:<port>/ should land on Swagger.
    app.MapMethods("/", new[] { HttpMethods.Get, HttpMethods.Head }, () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

// ============================================
// BL-015: must be placed before MapControllers()
// ============================================
app.UseRateLimiter();

app.MapControllers();

app.Run();

// -------------------------
// Local helper methods
// -------------------------

static async Task EnsureDatabaseReadyAsync(WebApplication app, IConfiguration configuration)
{
    // Auto migrate on startup (dev)
    const int maxAttempts = 15;
    const int delaySeconds = 2;

    // Why: In dev, when SQL Server runs in Docker, it may not be ready immediately.
    // We retry migrations to avoid crash loops and reduce "works on my machine" issues.
    Exception? lastError = null;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            // Why: Create a fresh scope + DbContext per attempt.
            // If the connection fails, reusing the same DbContext can keep a broken state.
            using IServiceScope scope = app.Services.CreateScope();
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Why: Confirm the actual resolved connection EF Core is using at runtime.
            // This helps detect cases where AddInfrastructure() config differs from what we think.
            DbConnection dbConn = db.Database.GetDbConnection();
            app.Logger.LogInformation(
                "[DEV] EF DbConnection: DataSource={DataSource}, Database={Database}",
                dbConn.DataSource,
                dbConn.Database);

            await SeedData.SeedAsync(scope.ServiceProvider, configuration);

            app.Logger.LogInformation("[DEV] Database migration + seed completed.");
            lastError = null;
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts && IsTransientDbStartupError(ex))
        {
            lastError = ex;

            app.Logger.LogWarning(
                "[DEV] DB not ready (attempt {Attempt}/{MaxAttempts}). Retrying in {DelaySeconds}s. Root: {RootCause}",
                attempt,
                maxAttempts,
                delaySeconds,
                GetRootCauseMessage(ex));

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }

    // If all retries failed, crash with a clear message (dev).
    if (lastError is not null)
    {
        throw new InvalidOperationException(
            "Database is not reachable after multiple attempts. " +
            "Make sure SQL Server Docker is running and your connection string is correct.",
            lastError);
    }
}

static bool IsTransientDbStartupError(Exception ex)
{
    // Why:
    // EF Core often wraps SqlException inside InvalidOperationException, DbUpdateException,
    // or other exceptions. We treat these as transient during container warm-up.
    Exception? root = ex;
    while (root.InnerException is not null)
    {
        root = root.InnerException;
    }

    return root is SqlException
           || root is TimeoutException
           || root is SocketException;
}

static string GetRootCauseMessage(Exception ex)
{
    Exception? root = ex;
    while (root.InnerException is not null)
    {
        root = root.InnerException;
    }

    return root.Message;
}

// Expose Program for integration testing (WebApplicationFactory).
public partial class Program
{
}
