using Aspire.AspNet.Web.Api.Services;
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
        builder.Services.AddGrpc().AddJsonTranscoding();
        builder.Services.AddGrpcReflection();

        builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
        }));
    }

    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler();
            app.UseHsts();
        }

        app.MapOpenApi();
        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
        app.UseCors();

        app.MapDefaultEndpoints();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGrpcService<WeatherServiceV1>().RequireCors("AllowAll");
        app.MapGrpcService<WeatherServiceV2>().RequireCors("AllowAll");

        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }

        app.MapGet("/", () => "This gRPC service is gRPC-Web enabled and is callable from browser apps using the gRPC-Web protocol");
    }
}
