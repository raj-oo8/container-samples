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
        private readonly WeatherService.WeatherServiceClient _weatherServiceClient;

        public HomeController(WeatherService.WeatherServiceClient weatherServiceClient)
        {
            _weatherServiceClient = weatherServiceClient;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public IActionResult Index()
        {
            var weatherForecastResponse = _weatherServiceClient.GetWeatherForecast(new WeatherForecastRequest());
            IEnumerable<WeatherForecast> forecasts = weatherForecastResponse.Forecasts;
            return View(forecasts);
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
