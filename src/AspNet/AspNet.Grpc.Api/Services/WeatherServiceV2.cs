using AspNet.Library.Protos;
using Grpc.Core;

namespace AspNet.Grpc.Api.Services
{
    //[Authorize]
    public class WeatherServiceV2 : WeatherRpcServiceV2.WeatherRpcServiceV2Base
    {
        private readonly CosmosDbService _cosmosDbService;

        public WeatherServiceV2(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        public override async Task<WeatherForecastResponseV2> GetWeatherForecast(WeatherForecastRequestV2 request, ServerCallContext context)
        {
            var summaries = await _cosmosDbService.GetSummariesAsync();

            var forecasts = new List<WeatherForecastV2>();

            if (summaries.Count > 0)
            {
                forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastV2
                {
                    Date = DateTime.Now.AddDays(index).ToString("dd-MMM-yyyy"),
                    TemperatureC = Random.Shared.Next(-40, 70),
                    TemperatureF = Random.Shared.Next(-40, 158),
                    Humidity = Random.Shared.Next(0, 100),
                    Summary = summaries[Random.Shared.Next(summaries.Count)].SummaryText
                }).ToList();
            }

            var response = new WeatherForecastResponseV2();
            response.Forecasts.AddRange(forecasts);

            return await Task.FromResult(response);
        }
    }
}
