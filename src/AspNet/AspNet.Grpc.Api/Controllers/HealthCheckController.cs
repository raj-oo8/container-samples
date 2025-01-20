using AspNet.Grpc.Api.Services;
using AspNet.Library.Protos;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace AspNet.Grpc.Api.Controllers
{
    [ApiController]
    [Route("healthz")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet("ready")]
        public async Task<IActionResult> GetReadinessStatus()
        {
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
    }
}
