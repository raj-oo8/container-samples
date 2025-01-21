using AspNet.Library.Protos;
using Grpc.Core;

namespace AspNet.Grpc.Api.Services
{
    //[Authorize]
    public class WeatherServiceV1 : WeatherRpcServiceV1.WeatherRpcServiceV1Base
    {
        private readonly CosmosDbService _cosmosDbService;

        public WeatherServiceV1(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        public override async Task<WeatherForecastResponseV1> GetWeatherForecast(WeatherForecastRequestV1 request, ServerCallContext context)
        {
            var summaries = await _cosmosDbService.GetSummariesAsync();
            var forecasts = new List<WeatherForecastV1>();

            if (summaries.Count > 0)
            {
                forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastV1
                {
                    Date = DateTime.Now.AddDays(index).ToString("dd-MMM-yyyy"),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Count)].SummaryText
                }).ToList();
            }

            var response = new WeatherForecastResponseV1();
            response.Forecasts.AddRange(forecasts);

            return await Task.FromResult(response);
        }
    }
}
