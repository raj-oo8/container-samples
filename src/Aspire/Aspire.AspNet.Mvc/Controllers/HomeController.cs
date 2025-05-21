using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aspire.AspNet.Mvc.Models;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Aspire.AspNet.Library.Models;

namespace Aspire.AspNet.Mvc.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDownstreamApi _downstreamApi;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IDownstreamApi downstreamApi, IConfiguration configuration)
    {
        _logger = logger;
        _downstreamApi = downstreamApi;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Read ClientId from configuration
            var clientId = _configuration["AzureAd:ClientId"];
            var scope = _configuration["DownstreamApi:Scopes"];

            if (string.IsNullOrEmpty(scope))
            {
                throw new InvalidOperationException("The scope cannot be null or empty. Please check the configuration.");
            }

            using var response = await _downstreamApi.CallApiForUserAsync(
                "DownstreamApi",
                options =>
                {
                    options.Scopes = [scope]; // Ensure scope is not null
                    options.RelativePath = "weatherforecast";
                }
            ).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>().ConfigureAwait(false);
                return View(forecasts);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
            }
        }
        catch (Microsoft.Identity.Client.MsalUiRequiredException ex)
        {
            _logger.LogError(ex, "Token acquisition failed. Redirecting to login.");
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException ex)
        {
            _logger.LogError(ex, "User challenge required. Redirecting to login.");
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
