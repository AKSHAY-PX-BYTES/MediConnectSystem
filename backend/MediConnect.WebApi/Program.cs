using System.Text;
using MediConnect.Application;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Infrastructure;
using MediConnect.Infrastructure.Persistence;
using MediConnect.Infrastructure.Security;
using MediConnect.WebApi.Middleware;
using MediConnect.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----- Configuration: allow env-var overrides (e.g. Neon connection string) -----
builder.Configuration.AddEnvironmentVariables();

// ----- Services -----
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// Current-user / tenant context (one instance shared by both interfaces per request).
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserService>());
builder.Services.AddScoped<ICurrentTenantProvider>(sp => sp.GetRequiredService<CurrentUserService>());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ----- JWT Authentication -----
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
          ?? new JwtSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ----- CORS for the Angular SPA -----
const string SpaCors = "SpaCors";
builder.Services.AddCors(options =>
    options.AddPolicy(SpaCors, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? new[] { "http://localhost:4200" })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

// ----- Swagger with JWT support -----
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MediConnect SaaS API",
        Version = "v1",
        Description = "Multi-tenant Clinic & Doctor Appointment Management Platform"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

// ----- Pipeline -----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediConnect API v1"));
}

app.UseHttpsRedirection();
app.UseCors(SpaCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new { service = "MediConnect API", status = "running" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timeUtc = DateTime.UtcNow }));

// ----- Apply migrations & seed on startup -----
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        await DbSeeder.SeedAsync(db, hasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration/seed failed on startup.");
    }
}

app.Run();
