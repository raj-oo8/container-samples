using AspNet.Library.Protos;
using AspNet.Mvc.Models;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Diagnostics;

namespace AspNet.Mvc.Controllers
{
    [Authorize]
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

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public IActionResult Index()
        {
            var methodName = nameof(Index);
            _logger.LogInformation(eventId, $"Starting {methodName}...");

            var baseUrl = _configuration["DownstreamApi:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                try
                {
                    if (!_previewService.IsPreviewEnabled)
                    {
                        var client = new WeatherRpcServiceV1.WeatherRpcServiceV1Client(GrpcChannel.ForAddress(baseUrl));
                        var weatherForecastResponse = client.GetWeatherForecast(new WeatherForecastRequestV1());
                        IEnumerable<WeatherForecastV1> forecasts = weatherForecastResponse.Forecasts;

                        _logger.LogInformation(eventId, $"Fetched {nameof(WeatherForecastV1)} of {methodName} successfully");

                        return View(forecasts);
                    }
                    else
                    {
                        var client = new WeatherRpcServiceV2.WeatherRpcServiceV2Client(GrpcChannel.ForAddress(baseUrl));
                        var weatherForecastResponse = client.GetWeatherForecast(new WeatherForecastRequestV2());
                        IEnumerable<WeatherForecastV2> forecasts = weatherForecastResponse.Forecasts;

                        _logger.LogInformation(eventId, $"Fetched {nameof(WeatherForecastV2)} of {methodName} successfully");

                        return View("IndexPreview", forecasts);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(eventId, ex, $"Error in {methodName}: {ex.Message}");
                }
            }
            return View(new List<WeatherForecastV1>());
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
    }
}
