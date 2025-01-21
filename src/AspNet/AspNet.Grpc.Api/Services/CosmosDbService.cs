using AspNet.Grpc.Api.Models;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AspNet.Grpc.Api.Services
{
    public class CosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<CosmosDbService> _logger;

        public CosmosDbService(IConfiguration configuration, ILogger<CosmosDbService> logger)
        {
            var accountEndpoint = configuration["CosmosDb:Endpoint"];
            var tenantId = configuration["AzureAd:TenantId"];

            if (string.IsNullOrEmpty(accountEndpoint))
            {
                throw new ArgumentException("CosmosDb:Endpoint configuration value is missing or empty.");
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("AzureAd:TenantId configuration value is missing or empty.");
            }

            var credentialOptions = new DefaultAzureCredentialOptions
            {
                VisualStudioTenantId = tenantId
            };

            _cosmosClient = new CosmosClient(accountEndpoint, new DefaultAzureCredential(credentialOptions));
            _container = _cosmosClient.GetContainer("Weather", "Summaries");
            _logger = logger;
        }


        public async Task<List<Summary>> GetSummariesAsync()
        {
            var summaries = new List<Summary>();

            try
            {
                var queryable = _container.GetItemLinqQueryable<Summary>();
                using FeedIterator<Summary> linqFeed = queryable.ToFeedIterator();
                while (linqFeed.HasMoreResults)
                {
                    FeedResponse<Summary> response = await linqFeed.ReadNextAsync();

                    // Iterate query results
                    summaries.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error fetching summaries: {Message}", ex.Message);
                throw;
            }

            return summaries;
        }
    }
}
