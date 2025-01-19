using AspNet.Library.Protos;
using AspNet.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics;

namespace AspNet.Mvc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly WeatherRpcServiceV1.WeatherRpcServiceV1Client _weatherRpcServiceV1Client;
        private readonly WeatherRpcServiceV2.WeatherRpcServiceV2Client _weatherRpcServiceV2Client;
        private readonly IPreviewService _previewService;

        public HomeController(
            WeatherRpcServiceV1.WeatherRpcServiceV1Client weatherRpcServiceV1Client, 
            WeatherRpcServiceV2.WeatherRpcServiceV2Client weatherRpcServiceV2Client,
            IPreviewService previewService)
        {
            _weatherRpcServiceV1Client = weatherRpcServiceV1Client;
            _weatherRpcServiceV2Client = weatherRpcServiceV2Client;
            _previewService = previewService;

        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public IActionResult Index()
        {
            if (!_previewService.IsPreviewEnabled)
            {
                var weatherForecastResponse = _weatherRpcServiceV1Client.GetWeatherForecast(new WeatherForecastRequestV1());
                IEnumerable<WeatherForecastV1> forecasts = weatherForecastResponse.Forecasts;
                return View(forecasts);
            }
            else
            {
                var weatherForecastResponse = _weatherRpcServiceV2Client.GetWeatherForecast(new WeatherForecastRequestV2());
                IEnumerable<WeatherForecastV2> forecasts = weatherForecastResponse.Forecasts;
                return View("IndexPreview", forecasts);
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
}
