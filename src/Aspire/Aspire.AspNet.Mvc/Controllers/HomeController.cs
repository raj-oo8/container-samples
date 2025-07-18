using Aspire.AspNet.Library.Models;
using Aspire.AspNet.Library.Protos;
using Aspire.AspNet.Mvc.Models;
using Google.Protobuf;
using Grpc.Core;
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

public class HomeController : Controller
{
    readonly EventId eventId = new(200, typeof(HomeController).FullName);
    readonly IDownstreamApi _downstreamApi;
    //readonly ITokenAcquisition _tokenAcquisition;
    readonly ILogger<HomeController> _logger;
    readonly IConfiguration _configuration;
    //readonly WeatherRpcServiceV1.WeatherRpcServiceV1Client _weatherRpcServiceV1Client;
    //readonly WeatherRpcServiceV2.WeatherRpcServiceV2Client _weatherRpcServiceV2Client;

    public HomeController
    (
        ILogger<HomeController> logger, 
        IDownstreamApi downstreamApi, 
        IConfiguration configuration 
        //WeatherRpcServiceV1.WeatherRpcServiceV1Client weatherRpcServiceV1Client,
        //WeatherRpcServiceV2.WeatherRpcServiceV2Client weatherRpcServiceV2Client,
        //ITokenAcquisition tokenAcquisition
    )
    {
        _logger = logger;
        _downstreamApi = downstreamApi;
        _configuration = configuration;
        //_weatherRpcServiceV1Client = weatherRpcServiceV1Client;
        //_weatherRpcServiceV2Client = weatherRpcServiceV2Client;
        //_tokenAcquisition = tokenAcquisition;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Weather()
    {
        var scope = _configuration["DownstreamApi:Scopes"];
        var baseUrl = _configuration["DownstreamApi:BaseUrl"];
        try
        {
            var isPreviewEnabled = _configuration["IsPreviewEnabled"];

            if (bool.Parse(isPreviewEnabled))
            {
                var forecasts = await GetWeatherForecastV2Async(scope);
                return View("WeatherPreview", forecasts);
            }
            else
            {
                var forecasts = await GetWeatherForecastV1Async(scope);
                var weatherForecasts = forecasts.Select(f => new WeatherForecast
                {
                    Date = DateOnly.Parse(f.Date),
                    TemperatureC = f.TemperatureC,
                    Summary = f.Summary
                }).ToList();
                return View(weatherForecasts);
            }
        }
        catch (MsalUiRequiredException ex)
        {
            _logger.LogError(eventId, ex, $"Error in {nameof(Index)}: {ex.Message}");
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _logger.LogError(eventId, ex, $"Error in {nameof(Index)}: {ex.Message}");
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(eventId, ex, $"Error in {nameof(Index)}: {ex.Message}");
            var forecasts = await GetWeatherForecastAsync(scope);
            return View(forecasts);
        }
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

        //var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { scope });
        //var headers = new Metadata
        //{
        //    { "Authorization", $"Bearer {accessToken}" }
        //};
        //var response = await _weatherRpcServiceV1Client.GetWeatherForecastAsync(new WeatherForecastRequestV1(), headers);

        //if (response != null)
        //{
        //    if (response.Forecasts != null)
        //    {
        //        forecasts.AddRange(response.Forecasts);
        //    }
        //}

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

            if (result.Forecasts != null)
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

    async Task<List<WeatherForecastV2>> GetWeatherForecastV2Async(string scope)
    {
        var forecasts = new List<WeatherForecastV2>();

        //var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { scope });
        //var headers = new Metadata
        //{
        //    { "Authorization", $"Bearer {accessToken}" }
        //};
        //var response = await _weatherRpcServiceV2Client.GetWeatherForecastAsync(new WeatherForecastRequestV2(), headers);

        //if (response != null)
        //{
        //    if (response.Forecasts != null)
        //    {
        //        forecasts.AddRange(response.Forecasts);
        //    }
        //}

        using var response = await _downstreamApi.CallApiForUserAsync
        (
            "DownstreamApi",
            options =>
            {
                options.Scopes = [scope];
                options.RelativePath = "v2/weatherforecast";
            }
        ).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            var result = parser.Parse<WeatherForecastResponseV2>(json);

            if (result.Forecasts != null)
            {
                forecasts.AddRange(result.Forecasts);
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogError(eventId, error, $"Error in {nameof(GetWeatherForecastV2Async)}: {response.StatusCode}");
        }

        return forecasts;
    }

    async Task<List<WeatherForecast>> GetWeatherForecastAsync(string scope)
    {
        var forecasts = new List<WeatherForecast>();

        using var response = await _downstreamApi.CallApiForUserAsync
        (
            "DownstreamApi",
            options =>
            {
                options.Scopes = [scope];
                options.RelativePath = "weatherforecast";
            }
        ).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();
            if (result != null)
            {
                forecasts = result;
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(eventId, error, $"Error in {nameof(GetWeatherForecastAsync)}: {response.StatusCode}");
        }

        return forecasts;
    }
}
