using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace AspNet.Mvc.Controllers
{
    [ApiController]
    [Route("healthz")]
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
            _logger.LogInformation(eventId, $"Starting {nameof(GetReadinessStatus)}...");

            var host = HttpContext.Request.Host.Host;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host);
                if (reply.Status == IPStatus.Success)
                {
                    return Ok("Readiness check passed.");
                }
                else
                {
                    return StatusCode(503, "Readiness check failed.");
                }
            }
            catch
            {
                return StatusCode(503, "Readiness check failed.");
            }
        }

        [HttpGet("startup")]
        public IActionResult GetStartupStatus()
        {
            _logger.LogInformation(eventId, $"Starting {nameof(GetStartupStatus)}...");

            var apiUrl = _configuration["DownstreamApi:BaseUrl"];
            var tenantId = _configuration["AzureAd:TenantId"];

            if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(tenantId))
            {
                return StatusCode(503, "Startup check failed.");
            }

            return Ok("Startup check passed.");
        }
    }
}
