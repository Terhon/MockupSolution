using Microsoft.AspNetCore.Mvc;
using PollingService.Services;

namespace PollingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollingController : ControllerBase
    {
        private readonly IDataService _dataService;

        public PollingController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpPost("{clientId}")]
        public async Task<IActionResult> StartProcessing(string clientId)
        {
            var requestId = await _dataService.StartProcessingAsync(clientId);
            return Ok(new { requestId });
        }

        [HttpGet("result/{requestId}")]
        public async Task<IActionResult> GetResult(string requestId)
        {
            var result = await _dataService.GetResultAsync(requestId);
            if (result == null)
                return Accepted("Still processing...");

            return Ok(new { result });
        }
    }
}