using AspNet.Library.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace AspNet.GrpcApis.Services
{
    //[Authorize]
    public class WeatherServiceV1 : WeatherService.WeatherServiceBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        public override Task<WeatherForecastResponse> GetWeatherForecast(WeatherForecastRequest request, ServerCallContext context)
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index).ToString("dd-MMM-yyyy"),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToList();

            var response = new WeatherForecastResponse();
            response.Forecasts.AddRange(forecasts);

            return Task.FromResult(response);
        }
    }
}
