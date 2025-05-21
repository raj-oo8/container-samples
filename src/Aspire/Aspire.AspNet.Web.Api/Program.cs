using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Aspire.AspNet.Web.Api;

public class Program
{
    static readonly EventId eventId = new(100, typeof(Program).FullName);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureBuilder(builder);

        var app = builder.Build();

        try
        {
            app.Logger.LogInformation(eventId, "Starting api...");

            ConfigureApp(app);

            app.Logger.LogInformation(eventId, "Configured api successfully");

            app.Run();

            app.Logger.LogInformation(eventId, "Started api successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(eventId, ex, "Api failed to start");
            throw;
        }
    }

    private static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        // Add this to ensure user secrets are loaded (after appsettings, before env vars)
        builder.Configuration.AddUserSecrets<Program>(optional: true);

        // Add this to ensure environment variables (including ACA secrets) are loaded into configuration
        builder.Configuration.AddEnvironmentVariables();

        builder.AddServiceDefaults();
        // Add services to the container.
        builder.Services.AddProblemDetails();

        // Add services to the container.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
    }

    private static void ConfigureApp(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        app.UseExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapDefaultEndpoints();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}
