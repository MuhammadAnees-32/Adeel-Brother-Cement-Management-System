using AdeelBrotherCement.Application.Services;
using AdeelBrotherCement.Infrastructure.Excel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
app.UseAuthorization();
app.MapControllers();

app.Run();