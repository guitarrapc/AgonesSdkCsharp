using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AgonesMockServer.Controllers;

[Route("api/[controller]")]
public class FailureController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public FailureController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // curl -X GET http://localhost:9358/api/failure
    // curl -X POST http://localhost:9358/api/failure -d {}
    [HttpGet]
    [HttpPost()]
    public ActionResult Health()
    {
        _logger.LogInformation($"failure");
        return StatusCode((int)HttpStatusCode.InternalServerError, null);
    }
}
