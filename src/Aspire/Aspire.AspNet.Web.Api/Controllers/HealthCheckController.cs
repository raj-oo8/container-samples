using Aspire.AspNet.Library.Protos;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace Aspire.AspNet.Web.Api.Controllers
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
                    _logger.LogInformation(eventId, $"Status: {reply.Status}...");
                    return Ok("Readiness check passed.");
                }
                else
                {
                    _logger.LogInformation(eventId, $"Status: {reply.Status}...");
                    return StatusCode(503, "Readiness check failed.");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(eventId, $"Error: {ex.Message}...");
                return StatusCode(503, "Readiness check failed.");
            }
        }

        [HttpGet("live")]
        public IActionResult GetLivenessStatus()
        {
            _logger.LogInformation(eventId, $"Starting {nameof(GetLivenessStatus)}...");

            try
            {
                var client = new WeatherForecastController();
                var weatherForecastResponse = client.Get();
                var count = weatherForecastResponse.Count();
                if (count > 0)
                {
                    _logger.LogInformation(eventId, $"Weather forecast count: {count}...");
                    return Ok("Liveness check passed.");
                }
                else
                {
                    _logger.LogInformation(eventId, $"Weather forecast count: {count}...");
                    return StatusCode(503, "Liveness check failed.");
                }
            }
            catch( Exception ex )
            {
                _logger.LogError(eventId, $"Error: {ex.Message}...");
                return StatusCode(503, "Liveness check failed.");
            }
        }

        [HttpGet("startup")]
        public IActionResult GetStartupStatus()
        {
            _logger.LogInformation(eventId, $"Starting {nameof(GetStartupStatus)}...");

            var accountEndpoint = _configuration["CosmosDb:Endpoint"];
            var tenantId = _configuration["AzureAd:TenantId"];

            if (string.IsNullOrWhiteSpace(accountEndpoint) || string.IsNullOrWhiteSpace(tenantId))
            {
                return StatusCode(503, "Startup check failed.");
            }

            return Ok("Startup check passed.");
        }
    }
}
