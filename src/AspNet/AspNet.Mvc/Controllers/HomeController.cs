using AspNet.Library.Protos;
using AspNet.Mvc.Models;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics;

namespace AspNet.Mvc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPreviewService _previewService;
        private readonly IConfiguration _configuration;

        public HomeController(
            IConfiguration configuration,
            IPreviewService previewService)
        {
            _configuration = configuration;
            _previewService = previewService;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public IActionResult Index()
        {
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
                        return View(forecasts);
                    }
                    else
                    {
                        var client = new WeatherRpcServiceV2.WeatherRpcServiceV2Client(GrpcChannel.ForAddress(baseUrl));
                        var weatherForecastResponse = client.GetWeatherForecast(new WeatherForecastRequestV2());
                        IEnumerable<WeatherForecastV2> forecasts = weatherForecastResponse.Forecasts;
                        return View("IndexPreview", forecasts);
                    }
                }
                catch (Exception)
                {
                    
                }
            }
            return View(new List<WeatherForecastV1>());
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
}
