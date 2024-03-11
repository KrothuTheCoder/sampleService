using Microsoft.AspNetCore.Mvc;

namespace DoSomethingService.Controllers.v2;

/// <summary>
/// This is an api to return some data in a made up senario
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[ApiVersion("2.0")]
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
    [ApiExplorerSettings(GroupName ="v2")]
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
    [HttpGet("WhatSomethingToDo")]
    [ApiExplorerSettings(GroupName ="v2")]
    public string Get(int whatToDo)
    {
        return $"You wanted me to {whatToDo}, there I did something";
    }
   
    /// <summary>
    /// If you want to post something for it to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpPost("PostSomething")]
    [ApiExplorerSettings(GroupName ="v2")]
    public string Post(string whatToDo)
    {
        return $"This was passed to me {whatToDo}, there I did something";
    }

    /// <summary>
    /// If you need to do something then, just tell it
    /// what you want to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpGet("WhatAnotherSomethingToDo")]
    [ApiExplorerSettings(GroupName ="v2")]
    public string GetAnother(int whatToDo)
    {
        return $"You wanted me to {whatToDo}, there I did something";
    }
   
}