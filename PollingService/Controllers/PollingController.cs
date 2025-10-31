using Microsoft.AspNetCore.Mvc;
using PollingService.Services;

namespace PollingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollingController(IDataService dataService) : ControllerBase
    {
        [HttpGet("{clientId}")]
        public IActionResult Get(string clientId)
        {
            if (dataService.TryGetCached(clientId, out var result))
                return Ok(new Result(result));

            var requestId = dataService.StartFetchAsync(clientId);
            return AcceptedAtAction(nameof(GetResult), new {requestId}, new RequestData(requestId));
        }

        [HttpGet("result/{requestId}")]
        public IActionResult GetResult(string requestId)
        {
            if (!dataService.TryGetResult(requestId, out var result, out var completed))
                return NotFound(new ErrorResponse( "Request not found" ));
            if (!completed)
                return Accepted(new ProcessingResponse( "Still processing..."));
            return Ok(new Result(result));
        }
    }
    
    public record Result(string Data);
    public record RequestData(string RequestId);
    public record ErrorResponse(string Message);
    public record ProcessingResponse(string Message);
}
