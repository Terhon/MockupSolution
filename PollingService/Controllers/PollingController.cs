using Microsoft.AspNetCore.Mvc;
using PollingService.Services;

namespace PollingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollingController(IDataService dataService) : ControllerBase
    {
        [HttpGet("{clientId}")]
        public async Task<IActionResult> Get(string clientId)
        {
            if (dataService.TryGetResult(clientId, out var result))
                return Ok(new { result });

            var requestId = await dataService.StartProcessingAsync(clientId);
            return Accepted(new { requestId });
        }

        [HttpGet("result/{requestId}")]
        public IActionResult GetResult(string requestId)
        {
            if (!dataService.TryGetResult(requestId, out var result))
                return Accepted("Still processing...");

            return Ok(new { result });
        }
    }
}