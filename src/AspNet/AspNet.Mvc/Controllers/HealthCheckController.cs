using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace AspNet.Mvc.Controllers
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
    }
}
