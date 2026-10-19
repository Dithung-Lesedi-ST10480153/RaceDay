using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
options.SwaggerDoc("v1", new()
{
Title = "RaceDay API",
Version = "v1",
Description = "REST API for RaceDay event management."
});
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RaceDayDb")));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
{
options.Cookie.Name = "RaceDay.Auth";
options.Cookie.HttpOnly = true;
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Lax;

options.ExpireTimeSpan = TimeSpan.FromHours(2);
options.SlidingExpiration = true;

// Return HTTP status codes instead of browser redirects.
options.Events.OnRedirectToLogin = context =>
{
context.Response.StatusCode = StatusCodes.Status401Unauthorized;
return Task.CompletedTask;
};

options.Events.OnRedirectToAccessDenied = context =>
{
context.Response.StatusCode = StatusCodes.Status403Forbidden;
return Task.CompletedTask;
};
});

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required by WebApplicationFactory in the integration tests.
public partial class Program { }

namespace RaceDay.RaceDay
{
    public class Program
    {
    }
}
