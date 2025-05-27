using Aspire.AspNet.Library.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Aspire.AspNet.Web.Api.Services
{
    [Authorize(Policy = "APIAccessPolicy")]
    public class WeatherServiceV1 : WeatherRpcServiceV1.WeatherRpcServiceV1Base
    {
        public override async Task<WeatherForecastResponseV1> GetWeatherForecast(WeatherForecastRequestV1 request, ServerCallContext context)
        {
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

            return await Task.FromResult(response);
        }
    }
}
