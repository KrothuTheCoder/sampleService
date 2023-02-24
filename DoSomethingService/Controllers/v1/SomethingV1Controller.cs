using Microsoft.AspNetCore.Mvc;

namespace DoSomethingService.Controllers.v1;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("[controller]")]
public class SomethingV1Controller : ControllerBase
{
    private readonly ILogger<SomethingV1Controller> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public SomethingV1Controller(ILogger<SomethingV1Controller> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet(Name = "DoSomething")]
    public string Get(string input)
    {
        return $"This was passed to me {input}, there I did something";
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost(Name = "PostSomething")]
    public string Post(string input)
    {
        return $"This was passed to me {input}, there I did something";
    }
}