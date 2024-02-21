using Microsoft.AspNetCore.Mvc;

namespace DoSomethingService.Controllers.v1;

/// <summary>
/// This is an api to return some data in a made up senario
/// </summary>
[ApiController]
[Route("v1/something")]
public class SomethingV1Controller : ControllerBase
{
    private readonly ILogger<SomethingV1Controller> _logger;

    /// <summary>
    /// Setting up the code so it can do things
    /// </summary>
    /// <param name="logger">The logger so we know if things have been done</param>
    public SomethingV1Controller(ILogger<SomethingV1Controller> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// If you need to do something then, just tell it
    /// what you want to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpGet("DoSomething")]
    public string Get(string whatToDo)
    {
        return $"You wanted me to {whatToDo}, there I did something";
    }
    
    /// <summary>
    /// If you want to post something for it to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpPost("PostSomething")]
    public string Post(string whatToDo)
    {
        return $"This was passed to me {whatToDo}, there I did something";
    }
}