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

        [HttpGet("live")]
        public async Task<IActionResult> GetLivenessStatus()
        {
            _logger.LogInformation(eventId, $"Starting {nameof(GetLivenessStatus)}...");

            var request = HttpContext.Request;
            var host = $"{request.Scheme}://{request.Host.Value}";

            if (string.IsNullOrWhiteSpace(host))
            {
                return StatusCode(503, "Liveness check failed.");
            }

            try
            {
                var client = new WeatherRpcServiceV1.WeatherRpcServiceV1Client(GrpcChannel.ForAddress(host));
                var weatherForecastResponse = await client.GetWeatherForecastAsync(new WeatherForecastRequestV1());
                if (weatherForecastResponse.Forecasts.Count() > 0)
                {
                    return Ok("Liveness check passed.");
                }
                else
                {
                    return StatusCode(503, "Liveness check failed.");
                }
            }
            catch
            {
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
