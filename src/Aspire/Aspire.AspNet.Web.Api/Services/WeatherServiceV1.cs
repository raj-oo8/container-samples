using Aspire.AspNet.Library.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;

namespace Aspire.AspNet.Web.Api.Services
{
    [Authorize(Policy = "APIAccessPolicy")]
    public class WeatherServiceV1 : WeatherRpcServiceV1.WeatherRpcServiceV1Base
    {
        readonly EventId eventId = new(200, typeof(WeatherServiceV1).FullName);
        readonly ILogger<WeatherServiceV1> _logger;

        public WeatherServiceV1(ILogger<WeatherServiceV1> logger)
        {
            _logger = logger;
        }

        public override async Task<WeatherForecastResponseV1> GetWeatherForecast(WeatherForecastRequestV1 request, ServerCallContext context)
        {
            var methodName = nameof(GetWeatherForecast);
            _logger.LogInformation(eventId, $"Starting {methodName}...");

            var summaries = new List<string>
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
            var forecasts = new List<WeatherForecastV1>();

            forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastV1
            {
                Date = DateTime.Now.AddDays(index).ToString("dd-MMM-yyyy"),
                TemperatureC = Random.Shared.Next(-5, 40),
                Summary = summaries.Count > 0 ? summaries[Random.Shared.Next(summaries.Count)] : string.Empty
            }).ToList();

            var response = new WeatherForecastResponseV1();
            response.Forecasts.AddRange(forecasts);

            _logger.LogInformation(eventId, $"Fetched {methodName} successfully");

            return await Task.FromResult(response);
        }
    }
}
