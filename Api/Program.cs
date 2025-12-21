using System.Text;

using Api.Auth;

using Infrastructure;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

const string corsPolicyName = "DevCors";

builder.Services.AddControllers();

// Bind JWT options (strongly typed)
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
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
        Description = "Paste JWT token: Bearer {token}"
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// DI Infrastructure (DbContext, repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// ---------------------------
// Authentication / Authorization (JWT)
// ---------------------------
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    // Fail fast: JWT key là bắt buộc để Auth hoạt động.
    throw new InvalidOperationException("Missing Jwt:Key in configuration.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            // Dùng claim "sub" làm định danh chính (Guid userId).
            NameClaimType = "sub"
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
