using Microsoft.AspNetCore.Mvc;

namespace DoSomethingService.Controllers.v2;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("v2/something")]
public class SomethingV2Controller : ControllerBase
{
    private readonly ILogger<SomethingV2Controller> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public SomethingV2Controller(ILogger<SomethingV2Controller> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("DoSomething")]
    public string Get(string input, string otherInput)
    {
        return $"This was passed to me {input}, there I did something then I did {otherInput}";
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("PostSomething")]
    public string Post(string input)
    {
        return $"This was passed to me {input}, there I did something";
    }
}