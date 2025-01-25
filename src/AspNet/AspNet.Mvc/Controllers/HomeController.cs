using AspNet.Library.Protos;
using AspNet.Mvc.Models;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics;

namespace AspNet.Mvc.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        readonly IPreviewService _previewService;
        readonly IConfiguration _configuration;
        readonly ILogger<HomeController> _logger;
        readonly EventId eventId = new(200, typeof(HomeController).FullName);

        public HomeController(
            IConfiguration configuration,
            IPreviewService previewService,
            ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _previewService = previewService;
            _logger = logger;
        }

        //[AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Index()
        {
            var methodName = nameof(Index);
            _logger.LogInformation(eventId, $"Starting {methodName}...");

            var baseUrl = _configuration["DownstreamApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return View(new List<WeatherForecastV1>());
            }

            if (!_previewService.IsPreviewEnabled)
            {
                var forecasts = await GetWeatherForecastsV1Async(baseUrl, methodName);
                return View(forecasts);
            }
            else
            {
                var forecastsV2 = await GetWeatherForecastsV2Async(baseUrl, methodName);
                return View("IndexPreview", forecastsV2);
            }
        }

        public IActionResult Privacy()
        {
            var methodName = nameof(Privacy);
            _logger.LogInformation(eventId, $"Starting {methodName}...");

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        async Task<List<WeatherForecastV1>> GetWeatherForecastsV1Async(string baseUrl, string methodName)
        {
            var forecasts = new List<WeatherForecastV1>();
            try
            {
                var client = new WeatherRpcServiceV1.WeatherRpcServiceV1Client(GrpcChannel.ForAddress(baseUrl));
                var weatherForecastResponse = await client.GetWeatherForecastAsync(new WeatherForecastRequestV1());
                forecasts.AddRange(weatherForecastResponse.Forecasts.ToList());

                _logger.LogInformation(eventId, $"Fetched {nameof(WeatherForecastV1)} of {methodName} using gRPC successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(eventId, ex, $"Error in {methodName}: {ex.Message}");
                forecasts = await FallbackToWebApiV1Async(forecasts, baseUrl, methodName);
            }
            return forecasts;
        }

        async Task<List<WeatherForecastV1>> FallbackToWebApiV1Async(List<WeatherForecastV1> forecasts, string baseUrl, string methodName)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{baseUrl}/v1/weather/forecast");
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecastV1>>().Result;

                if (result != null)
                {
                    forecasts.AddRange(result);
                }

                _logger.LogInformation(eventId, $"Fetched {nameof(WeatherForecastV1)} of {methodName} using REST successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(eventId, ex, $"Error in {methodName}: {ex.Message}");
            }
            return forecasts;
        }

        async Task<List<WeatherForecastV2>> GetWeatherForecastsV2Async(string baseUrl, string methodName)
        {
            var forecasts = new List<WeatherForecastV2>();
            try
            {
                var client = new WeatherRpcServiceV2.WeatherRpcServiceV2Client(GrpcChannel.ForAddress(baseUrl));
                var weatherForecastResponse = await client.GetWeatherForecastAsync(new WeatherForecastRequestV2());
                forecasts.AddRange(weatherForecastResponse.Forecasts.ToList());

                _logger.LogInformation(eventId, $"Fetched {nameof(WeatherForecastV2)} of {methodName} using gRPC successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(eventId, ex, $"Error in {methodName}: {ex.Message}");
                forecasts = await FallbackToWebApiV2Async(forecasts, baseUrl, methodName);
            }
            return forecasts;
        }

        async Task<List<WeatherForecastV2>> FallbackToWebApiV2Async(List<WeatherForecastV2> forecasts, string baseUrl, string methodName)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{baseUrl}/v2/weather/forecast");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecastV2>>();

                if (result != null)
                {
                    forecasts.AddRange(result);
                }

                _logger.LogInformation(eventId, $"Fetched {nameof(WeatherForecastV2)} of {methodName} using REST successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(eventId, ex, $"Error in {methodName}: {ex.Message}");
            }
            return forecasts;
        }
    }
}
