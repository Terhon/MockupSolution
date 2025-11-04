using Microsoft.AspNetCore.Mvc;
using ProcessingService.Models;

namespace ProcessingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        [HttpGet("{clientId}")]
        public IActionResult StartTask(string clientId)
        {
            var requestId = Guid.NewGuid().ToString();
            var count = ResultStore.RequestCount;

            if (count % 10 == 0)
                return StatusCode(500, $"Simulated error: #{clientId}");

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(60));

                ResultStore.Data.TryAdd(requestId, $"Unique result for {clientId} at {DateTime.UtcNow}");
            });

            return Ok(new RequestId(requestId));
        }

        [HttpGet("result/{requestId}")]
        public IActionResult GetResult(string requestId)
        {
            if (!ResultStore.Data.TryGetValue(requestId, out var result))
                return Accepted(new RequestStatus("Processing"));

            return Ok(new RequestResult(result));
        }
    }

    public record RequestId(string Id);
    public record RequestStatus(string Status);
    public record RequestResult(string Result);
}