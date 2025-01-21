using AspNet.Library.Protos;
using Grpc.Core;

namespace AspNet.Grpc.Api.Services
{
    //[Authorize]
    public class WeatherServiceV2 : WeatherRpcServiceV2.WeatherRpcServiceV2Base
    {
        private readonly CosmosDbService _cosmosDbService;
        readonly EventId eventId = new(200, typeof(WeatherServiceV2).FullName);
        readonly ILogger<WeatherServiceV2> _logger;

        public WeatherServiceV2(CosmosDbService cosmosDbService, ILogger<WeatherServiceV2> logger)
        {
            _logger = logger;
            _logger.LogInformation(eventId, "Starting service...");

            _cosmosDbService = cosmosDbService;

            _logger.LogInformation(eventId, "Started service successfully");
        }

        public override async Task<WeatherForecastResponseV2> GetWeatherForecast(WeatherForecastRequestV2 request, ServerCallContext context)
        {
            var methodName = nameof(GetWeatherForecast);
            _logger.LogInformation(eventId, $"Starting {methodName}...");

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

                _logger.LogInformation(eventId, $"Fetched {methodName} successfully");
            }
            else
            {
                _logger.LogWarning(eventId, $"No summaries found for {methodName}");
            }

            var response = new WeatherForecastResponseV2();
            response.Forecasts.AddRange(forecasts);

            return await Task.FromResult(response);
        }
    }
}
