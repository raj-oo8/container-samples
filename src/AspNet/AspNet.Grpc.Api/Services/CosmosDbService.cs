using AspNet.Grpc.Api.Models;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AspNet.Grpc.Api.Services
{
    public class CosmosDbService
    {
        readonly CosmosClient? _cosmosClient;
        readonly Container? _container;
        readonly ILogger<CosmosDbService> _logger;
        readonly EventId eventId = new(200, typeof(CosmosDbService).FullName);

        public CosmosDbService(IConfiguration configuration, ILogger<CosmosDbService> logger)
        {
            _logger = logger;
            _logger.LogInformation(eventId, "Starting service...");

            var accountEndpoint = configuration["CosmosDb:Endpoint"];
            var tenantId = configuration["AzureAd:TenantId"];

            if (string.IsNullOrWhiteSpace(accountEndpoint))
            {
                _logger.LogError(eventId, "CosmosDb:Endpoint configuration value is missing or empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                _logger.LogError(eventId, "AzureAd:TenantId configuration value is missing or empty.");
                return;
            }

            //_cosmosClient = new CosmosClient(accountEndpoint, new DefaultAzureCredential());
            //_container = _cosmosClient.GetContainer("Weather", "Summaries");

            _logger.LogInformation(eventId, "Started service successfully");
        }


        internal async Task<List<Summary>> GetSummariesAsync()
        {
            var summaries = new List<Summary>();
            var methodName = nameof(GetSummariesAsync);

            try
            {
                if (_container != null)
                {
                    var queryable = _container.GetItemLinqQueryable<Summary>();
                    using FeedIterator<Summary> linqFeed = queryable.ToFeedIterator();
                    while (linqFeed.HasMoreResults)
                    {
                        FeedResponse<Summary> response = await linqFeed.ReadNextAsync();
                        summaries.AddRange(response);
                    }
                }
                else
                {
                    _logger.LogError(eventId, $"Error in {methodName}: {nameof(Container)} object is missing or empty.");
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError(eventId, ex, $"Error in {methodName}: {ex.Message}");
            }

            return summaries;
        }
    }
}
