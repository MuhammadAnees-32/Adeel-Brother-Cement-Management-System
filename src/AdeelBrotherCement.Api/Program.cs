using System.Text;
using AdeelBrotherCement.Application.Services;
using AdeelBrotherCement.Infrastructure.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddExcelInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            return Task.CompletedTask;
        });
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

var wwwroot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (Directory.Exists(wwwroot))
{
    static void ApplyNoCache(HttpResponse response)
    {
        response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
    }

    var spaStaticFiles = new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            var path = ctx.Context.Request.Path.Value ?? string.Empty;

            if (path.Contains("app-version.json", StringComparison.OrdinalIgnoreCase)
                || ctx.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
            {
                ApplyNoCache(ctx.Context.Response);
                return;
            }

            if (path.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase))
                ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
    };

    app.UseDefaultFiles();
    app.UseStaticFiles(spaStaticFiles);
}

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            return Task.CompletedTask;
        });
    }

    await next();
});

app.MapControllers();

if (Directory.Exists(wwwroot))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers.Pragma = "no-cache";
            ctx.Context.Response.Headers.Expires = "0";
        }
    });
}

app.Run();
