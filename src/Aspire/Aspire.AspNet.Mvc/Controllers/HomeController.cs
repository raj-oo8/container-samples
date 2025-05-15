using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace WebApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDownstreamApi _downstreamApi;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IDownstreamApi downstreamApi)
    {
        _logger = logger;
        _downstreamApi = downstreamApi; ;
    }

    // filepath: c:\Users\rajo\Source\Repos\TwoTier\WebApp\Controllers\HomeController.cs
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public async Task<IActionResult> Index()
    {
        try
        {
            using var response = await _downstreamApi.CallApiForUserAsync("DownstreamApi").ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var forecasts = await response.Content.ReadFromJsonAsync<List<WebApp.ViewModels.WeatherForecastViewModel>>().ConfigureAwait(false);
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
