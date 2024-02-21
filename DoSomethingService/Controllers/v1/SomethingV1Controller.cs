using Microsoft.AspNetCore.Mvc;

namespace DoSomethingService.Controllers.v1;

/// <summary>
/// This is an api to return some data in a made up senario
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SomethingController : ControllerBase
{
    private readonly ILogger<SomethingController> _logger;

    /// <summary>
    /// Setting up the code so it can do things
    /// </summary>
    /// <param name="logger">The logger so we know if things have been done</param>
    public SomethingController(ILogger<SomethingController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// If you need to do something then it will
    /// </summary>
    /// <returns>What happened</returns>
    [HttpGet("DoSomething")]
    public string Get()
    {
        return $"You wanted me to do something, there I did something";
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
    /// If you need to do something then, just tell it
    /// what you want to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <param name="numberOfTimes">The number of times you want it to do something</param>
    /// <returns>What happened</returns>
    [MapToApiVersion("2.0")]
    public string Get(string whatToDo, int numberOfTimes)
    {
        var returnString = "";
        for (int i = 0; i < numberOfTimes; i++)
        {
            returnString += $"You wanted me to {whatToDo}, there I did something";
        }
        return returnString;
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