using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace Aspire.AspNet.Mvc.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        readonly EventId eventId = new(300, typeof(HealthCheckController).FullName);
        readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(IConfiguration configuration, ILogger<HealthCheckController> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> GetReadinessStatus()
        {
            _logger.LogInformation(eventId, $"Starting: {nameof(GetReadinessStatus)}...");

            var host = HttpContext.Request.Host.Host;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host);
                if (reply.Status == IPStatus.Success)
                {
                    _logger.LogInformation(eventId, $"Status: {reply.Status}");
                    return Ok("Readiness check passed.");
                }
                else
                {
                    _logger.LogInformation(eventId, $"Status: {reply.Status}");
                    return StatusCode(503, "Readiness check failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(eventId, $"Error: {ex.Message}");
                return StatusCode(503, "Readiness check failed.");
            }
        }


        [HttpGet("startup")]
        public IActionResult GetStartupStatus()
        {
            _logger.LogInformation(eventId, $"Starting {nameof(GetStartupStatus)}...");

            var domainConfig = "AzureAd:Domain";
            var domainValue = _configuration[domainConfig];
            if (string.IsNullOrWhiteSpace(domainValue))
            {
                _logger.LogInformation(eventId, $"Missing value: {domainConfig}");
                return StatusCode(503, "Startup check failed.");
            }

            var tenantConfig = "AzureAd:TenantId";
            var tenantValue = _configuration[tenantConfig];
            if (string.IsNullOrWhiteSpace(tenantValue))
            {
                _logger.LogInformation(eventId, $"Missing value: {tenantConfig}");
                return StatusCode(503, "Startup check failed.");
            }

            var clientConfig = "AzureAd:ClientId";
            var clientValue = _configuration[clientConfig];
            if (string.IsNullOrWhiteSpace(clientValue))
            {
                _logger.LogInformation(eventId, $"Missing value: {clientConfig}");
                return StatusCode(503, "Startup check failed.");
            }

            var secretConfig = "AzureAd:ClientSecret";
            var secretValue = _configuration[secretConfig];
            if (string.IsNullOrWhiteSpace(secretValue))
            {
                _logger.LogInformation(eventId, $"Missing value: {secretConfig}");
                return StatusCode(503, "Startup check failed.");
            }

            var scopeConfig = "DownstreamApi:Scopes";
            var scopeValue = _configuration[scopeConfig];
            if (string.IsNullOrWhiteSpace(scopeValue))
            {
                _logger.LogInformation(eventId, $"Missing value: {scopeConfig}");
                return StatusCode(503, "Startup check failed.");
            }

            _logger.LogInformation(eventId, $"No missing config values");
            return Ok("Startup check passed.");
        }
    }
}
