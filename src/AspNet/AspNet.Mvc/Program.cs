using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace AspNet.Mvc
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

        private static async Task ConfigureServicesAsync(WebApplicationBuilder builder)
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

            var entraIdClientSecret = builder.Configuration["KeyVault:EntraIdClientSecret"];

            if (!string.IsNullOrWhiteSpace(entraIdClientSecret))
            {
                builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(options =>
                    {
                        builder.Configuration.Bind("AzureAd", options);
                        options.ClientSecret = entraIdClientSecret;
                    });
            }

            bool isPreviewEnabled = builder.Configuration.GetValue<bool>("IsPreviewEnabled");
            builder.Services.AddSingleton<IPreviewService>(sp => new PreviewService(isPreviewEnabled));

            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();
        }

        private static void ConfigureApp(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();
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
