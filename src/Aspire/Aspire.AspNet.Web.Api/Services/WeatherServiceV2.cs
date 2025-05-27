using Aspire.AspNet.Library.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Aspire.AspNet.Web.Api.Services
{
    [Authorize(Policy = "APIAccessPolicy")]
    public class WeatherServiceV2 : WeatherRpcServiceV2.WeatherRpcServiceV2Base
    {
        public override async Task<WeatherForecastResponseV2> GetWeatherForecast(WeatherForecastRequestV2 request, ServerCallContext context)
        {
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

            return await Task.FromResult(response);
        }
    }
}
