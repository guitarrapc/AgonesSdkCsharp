using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AgonesMockServer.Controllers;

[Route("api/[controller]")]
public class CircuitController : Controller
{
    private readonly ILogger<HomeController> _logger;
    static int _counter = 0;

    public CircuitController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // curl -X GET http://localhost:9358/api/circuit?failure=3
    // curl -X POST http://localhost:9358/api/circuit -d {}
    // curl -X POST http://localhost:9358/api/circuit/3 -d {}
    [HttpGet]
    [HttpPost()]
    [HttpPost("{failure}")]
    public ActionResult Health(int failure = 4)
    {
        //   1:100% failure
        //   2: 50% failure
        //   3: 33% failure
        //   4: 25% failure
        //   5: 20% failure
        //  10: 10% failure
        //  20:  5% failure
        // 100:  1% failure
        _counter++;
        var isFailure = _counter % failure == 0;
        _logger.LogInformation($"circuit: failureRatio {failure}; counter {_counter}; is_failure {isFailure}");

        if (isFailure)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");
        }
        return Ok($"healthy");
    }
}
