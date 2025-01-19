using AspNet.Library.Protos;
using Grpc.Core;

namespace AspNet.Grpc.Api.Services
{
    //[Authorize]
    public class WeatherServiceV2 : WeatherRpcServiceV2.WeatherRpcServiceV2Base
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        public override Task<WeatherForecastResponseV2> GetWeatherForecast(WeatherForecastRequestV2 request, ServerCallContext context)
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastV2
            {
                Date = DateTime.Now.AddDays(index).ToString("dd-MMM-yyyy"),
                TemperatureC = Random.Shared.Next(-40, 70),
                TemperatureF = Random.Shared.Next(-40, 158),
                Humidity = Random.Shared.Next(0, 100),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToList();

            var response = new WeatherForecastResponseV2();
            response.Forecasts.AddRange(forecasts);

            return Task.FromResult(response);
        }
    }
}
