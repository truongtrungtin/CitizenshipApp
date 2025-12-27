using System.Text;

using Api.Auth;

using Infrastructure;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.Extensions.Options;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

const string corsPolicyName = "UiCors";

builder.Services.AddControllers();

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
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// DI Infrastructure (DbContext, repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// ---------------------------
// Authentication / Authorization (JWT)
// ---------------------------

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var jwtKey = builder.Configuration["Jwt:Key"]!;

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CitizenshipApp API v1");
        options.RoutePrefix = "swagger";
    });

    // Auto migrate on startup (dev)
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Migrate + seed (dev)
    db.Database.Migrate();
    await SeedData.SeedAsync(db);
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
