using Microsoft.AspNetCore.Mvc;
using ProcessingService.Models;

namespace ProcessingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        [HttpPost("{clientId}")]
        public IActionResult StartTask(string clientId)
        {
            var requestId = Guid.NewGuid().ToString();
            int count = ResultStore.RequestCount;

            if (count % 10 == 0)
                return StatusCode(500, $"Simulated error: request #{requestId}");
                
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(60));

                ResultStore.Data.TryAdd(requestId, $"Unique result for {clientId} at {DateTime.UtcNow}");
            });

            return Ok(new { requestId });
        }

        [HttpGet("result/{requestId}")]
        public IActionResult GetResult(string requestId)
        {
            if (!ResultStore.Data.TryGetValue(requestId, out var result))
                return StatusCode(202, new { status = "Processing" });

            return Ok(new { result });
        }
    }
}