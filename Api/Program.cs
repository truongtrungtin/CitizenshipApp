using Infrastructure;
using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

const string corsPolicyName = "DevCors";

builder.Services.AddControllers();

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

    // Add header auth: X-Admin-Key
    c.AddSecurityDefinition("AdminKey", new OpenApiSecurityScheme
    {
        Name = "X-Admin-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter Admin API Key"
    });

    // v10+ requires Func<OpenApiDocument, OpenApiSecurityRequirement>
    c.AddSecurityRequirement(doc =>
    {
        var requirement = new OpenApiSecurityRequirement();

        // OpenApiSecurityRequirement : Dictionary<OpenApiSecuritySchemeReference, List<string>>
        requirement.Add(
            new OpenApiSecuritySchemeReference("AdminKey", doc),
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
    db.Database.Migrate();
}

string? adminKey = builder.Configuration["AdminApiKey"];

// Middleware check X-Admin-Key for /api
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api"))
    {
        if (string.IsNullOrWhiteSpace(adminKey))
        {
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsync("AdminApiKey is not configured.");
            return;
        }

        if (!ctx.Request.Headers.TryGetValue("X-Admin-Key", out var key) || key != adminKey)
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsync("Missing/invalid X-Admin-Key");
            return;
        }
    }

    await next();
});

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);

app.UseAuthorization();
app.MapControllers();

app.Run();
