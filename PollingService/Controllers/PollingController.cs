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
            if (dataService.TryGetCached(clientId, out var result))
                return Ok(new Result(result));

            await dataService.StartFetch(clientId);
            return Accepted();
        }
    }
    
    public record Result(string Data);
}
