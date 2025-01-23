using AspNet.Grpc.Api.Filters;
using AspNet.Grpc.Api.Services;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace AspNet.Grpc.Api
{
    public class Program
    {
        static readonly EventId eventId = new(100, typeof(Program).FullName);

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            await ConfigureServicesAsync(builder);

            var app = builder.Build();

            try
            {
                app.Logger.LogInformation(eventId, "Starting app...");

                ConfigureApp(app);

                app.Logger.LogInformation(eventId, "Configured app successfully");

                app.Run();

                app.Logger.LogInformation(eventId, "Started app successfully");
            }
            catch (Exception ex)
            {
                app.Logger.LogCritical(eventId, ex, "App failed to start");
                throw;
            }
        }

        static async Task ConfigureServicesAsync(WebApplicationBuilder builder)
        {
            builder.Configuration.AddEnvironmentVariables();

            var appInsightsConnectionString = builder.Configuration["KeyVault:AppInsightsConnectionString"];

            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
            {
                var clientSecret = await GetClientSecretFromKeyVaultAsync(builder.Configuration, appInsightsConnectionString);
                builder.Services.AddOpenTelemetry().UseAzureMonitor(options => {
                    options.ConnectionString = clientSecret;
                });
            }

            //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            builder.Services.AddControllers();
            builder.Services.AddGrpc().AddJsonTranscoding();
            builder.Services.AddGrpcSwagger();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddGrpcReflection();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherServiceV1", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WeatherServiceV2", Version = "v2" });
                c.DocumentFilter<RemoveVersionParameterFilter>();
            });

            builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));

            builder.Services.AddSingleton<CosmosDbService>();
        }

        static void ConfigureApp(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherServiceV1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "WeatherServiceV2");
                });
            }

            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
            app.UseCors();

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

        async static Task<string?> GetClientSecretFromKeyVaultAsync(ConfigurationManager configuration, string secretName)
        {
            var keyVaultUrl = configuration["KeyVault:Url"];

            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                return null;
            }

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}