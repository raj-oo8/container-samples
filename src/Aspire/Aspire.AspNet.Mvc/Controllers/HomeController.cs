using Aspire.AspNet.Library.Models;
using Aspire.AspNet.Library.Protos;
using Aspire.AspNet.Mvc.Models;
using Google.Protobuf;
using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net;

namespace Aspire.AspNet.Mvc.Controllers;

[Authorize]
public class HomeController : Controller
{
    readonly EventId eventId = new(200, typeof(HomeController).FullName);
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
            var scope = _configuration["DownstreamApi:Scopes"];

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new InvalidOperationException("The scope cannot be null or empty. Please check the configuration.");
            }

            var forecasts = await GetWeatherForecastV1Async(scope);
            return View(forecasts);
        }
        catch (MsalUiRequiredException ex)
        {
            _logger.LogError(ex, "Token acquisition failed. Redirecting to login.");
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
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
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        if (exception != null)
        {
            _logger.LogError(eventId, exception, $"Error in {exception.Source}: {exception?.Message}");
        }

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    async Task<List<WeatherForecastV1>> GetWeatherForecastV1Async(string scope)
    {
        var forecasts = new List<WeatherForecastV1>();

        using var response = await _downstreamApi.CallApiForUserAsync
        (
            "DownstreamApi",
            options =>
            {
                options.Scopes = [scope];
                options.RelativePath = "v1/weatherforecast";
            }
        ).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            var result = parser.Parse<WeatherForecastResponseV1>(json);

            if (result?.Forecasts != null)
            {
                forecasts.AddRange(result.Forecasts);
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogError(eventId, error, $"Error in {nameof(GetWeatherForecastV1Async)}: {response.StatusCode}");
        }

        return forecasts;
    }
}
