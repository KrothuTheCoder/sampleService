using Microsoft.AspNetCore.Mvc;

namespace DoSomethingService.Controllers;

[ApiController]
[Route("[controller]")]
public class SomethingController : ControllerBase
{
    private readonly ILogger<SomethingController> _logger;

    public SomethingController(ILogger<SomethingController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "DoSomething")]
    public string Get(string input)
    {
        return $"This was passed to me {input}, there I did something";
    }
    
    [HttpPost(Name = "PostSomething")]
    public string Post(string input)
    {
        return $"This was passed to me {input}, there I did something";
    }
}