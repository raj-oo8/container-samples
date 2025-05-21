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
        builder.Configuration.AddUserSecrets<Program>(optional: true);
        builder.Configuration.AddEnvironmentVariables();

        builder.AddServiceDefaults();
        builder.Services.AddProblemDetails();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
    }

    private static void ConfigureApp(WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapOpenApi();

        app.MapDefaultEndpoints();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}
