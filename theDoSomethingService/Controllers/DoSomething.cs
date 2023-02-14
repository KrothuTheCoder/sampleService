using Microsoft.AspNetCore.Mvc;

namespace theDoSomethingService.Controllers;

public class DoSomething : Controller
{
    // GET
    public IActionResult Index()
    {
        return Ok();
    }
}