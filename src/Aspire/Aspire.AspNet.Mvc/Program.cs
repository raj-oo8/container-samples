using Aspire.AspNet.Library.Protos;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Aspire.AspNet.Mvc;

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

    private static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        builder.Configuration.AddUserSecrets<Program>(optional: true);
        builder.Configuration.AddEnvironmentVariables();

        builder.AddServiceDefaults();

        //builder.Services.AddGrpcClient<WeatherRpcServiceV1.WeatherRpcServiceV1Client>(o =>
        //{
        //    o.Address = new Uri(builder.Configuration.GetSection("DownstreamApi:BaseUrl").Value);
        //});
        //builder.Services.AddGrpcClient<WeatherRpcServiceV2.WeatherRpcServiceV2Client>(o =>
        //{
        //    o.Address = new Uri(builder.Configuration.GetSection("DownstreamApi:BaseUrl").Value);
        //});

        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        builder.Services.AddOutputCache();

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
            .AddDistributedTokenCaches();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<MicrosoftIdentityOptions>(options =>
            {
                options.Events.OnAuthenticationFailed = context =>
                {
                    context.Response.Headers.Append("x-pii-logging", "true");
                    return Task.CompletedTask;
                };
            });
        }
    }

    private static void ConfigureApp(WebApplication app)
    {
        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();

        app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                context.Response.Cookies.Append(
                    "WebAppSession",
                    "active",
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });
            }
            await next();
        });

        app.Use(async (context, next) =>
        {
            // Only check for session cookie if the user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Only check for protected endpoints (not [AllowAnonymous])
                var endpoint = context.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

                if (!allowAnonymous && !context.Request.Cookies.ContainsKey("WebAppSession"))
                {
                    context.Response.Redirect("/MicrosoftIdentity/Account/SignOut");
                    return;
                }
            }
            await next();
        });

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();
    }
}
