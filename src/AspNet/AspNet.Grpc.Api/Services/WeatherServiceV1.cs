using AspNet.Library.Protos;
using Grpc.Core;

namespace AspNet.Grpc.Api.Services
{
    //[Authorize]
    public class WeatherServiceV1 : WeatherRpcServiceV1.WeatherRpcServiceV1Base
    {
        private readonly CosmosDbService _cosmosDbService;
        readonly EventId eventId = new(200, typeof(WeatherServiceV1).FullName);
        readonly ILogger<WeatherServiceV1> _logger;

        public WeatherServiceV1(CosmosDbService cosmosDbService, ILogger<WeatherServiceV1> logger)
        {
            _logger = logger;
            _logger.LogInformation(eventId, "Starting service...");

            _cosmosDbService = cosmosDbService;

            _logger.LogInformation(eventId, "Started service successfully");
        }

        public override async Task<WeatherForecastResponseV1> GetWeatherForecast(WeatherForecastRequestV1 request, ServerCallContext context)
        {
            var methodName = nameof(GetWeatherForecast);
            _logger.LogInformation(eventId, $"Starting {methodName}...");

            var summaries = await _cosmosDbService.GetSummariesAsync();
            var forecasts = new List<WeatherForecastV1>();

            forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastV1
            {
                Date = DateTime.Now.AddDays(index).ToString("dd-MMM-yyyy"),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries.Count > 0 ? summaries[Random.Shared.Next(summaries.Count)].SummaryText : string.Empty
            }).ToList();

            var response = new WeatherForecastResponseV1();
            response.Forecasts.AddRange(forecasts);

            _logger.LogInformation(eventId, $"Fetched {methodName} successfully");

            return await Task.FromResult(response);
        }
    }
}
