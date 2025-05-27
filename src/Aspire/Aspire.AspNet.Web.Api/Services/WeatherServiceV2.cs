using Aspire.AspNet.Library.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;

namespace Aspire.AspNet.Web.Api.Services
{
    [Authorize(Policy = "APIAccessPolicy")]
    public class WeatherServiceV2 : WeatherRpcServiceV2.WeatherRpcServiceV2Base
    {
        readonly EventId eventId = new(200, typeof(WeatherServiceV2).FullName);
        readonly ILogger<WeatherServiceV2> _logger;

        public WeatherServiceV2(ILogger<WeatherServiceV2> logger)
        {
            _logger = logger;
        }

        public override async Task<WeatherForecastResponseV2> GetWeatherForecast(WeatherForecastRequestV2 request, ServerCallContext context)
        {
            var methodName = nameof(GetWeatherForecast);
            _logger.LogInformation(eventId, $"Starting {methodName}...");

            var summaries = new List<string>
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
            var forecasts = new List<WeatherForecastV2>();

            forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastV2
            {
                Date = DateTime.Now.AddDays(index).ToString("dd-MMM-yyyy"),
                TemperatureC = Random.Shared.Next(-40, 70),
                Humidity = Random.Shared.Next(0, 100),
                Summary = summaries.Count > 0 ? summaries[Random.Shared.Next(summaries.Count)] : string.Empty
            }).ToList();

            var response = new WeatherForecastResponseV2();
            response.Forecasts.AddRange(forecasts);

            _logger.LogInformation(eventId, $"Fetched {methodName} successfully");

            return await Task.FromResult(response);
        }
    }
}
