using DoSomethingService.Configuration.Telemetry;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace DoSomethingService.Controllers.v1;

/// <summary>
///     This is an api to return some data in a made up senario
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[EnableCors]
public class SomethingController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly ITelemetryContext _telemetryContext;


    /// <summary>
    ///     Setting up the code so it can do things
    /// </summary>
    /// <param name="telemetryContext"></param>
    /// <param name="logger">The logger so we know if things have been done</param>
    public SomethingController(ITelemetryContext telemetryContext, ILogger logger)
    {
        _telemetryContext = telemetryContext;
        _logger = logger;
    }

    /// <summary>
    ///     If you need to do something then it will
    /// </summary>
    /// <returns>What happened</returns>
    [HttpGet("DoSomething")]
    [ApiExplorerSettings(GroupName = "v1")]
    public IActionResult Get()
    {
        using var someActivity = _telemetryContext.ActivitySource.StartActivity("DoSomething");

        var stuffToSendAppInsights = new Dictionary<string, string>();

        foreach (var header in Response.Headers) stuffToSendAppInsights.TryAdd(header.Key, header.Value.ToString());
        stuffToSendAppInsights.Add("EventName", "Api Call");
        stuffToSendAppInsights.Add("x-event-type", "Something");

        return ConvertToJsonObject("You wanted me to do something, there I did something");
    }


    /// <summary>
    ///     If you need to do something then it will
    /// </summary>
    /// <returns>What happened</returns>
    private ContentResult ConvertToJsonObject(string input)
    {
        //Response.Headers.Add("Access-Control-Allow-Origin", "https://web.hakabo.com");
        Response.Headers.Add("x-peters-test", "https://web.hakabo.com");
        var jsonString = "{\"key\":\"" + input + "\"}";
        //string jsonString = $"{\"key\":\"{input}\"}"; 
        return new ContentResult
        {
            Content = jsonString,
            ContentType = "application/json",
            StatusCode = 200 // You can set the status code as needed
        };
    }

    /// <summary>
    ///     If you need to do something then, just tell it
    ///     what you want to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpGet("WhatSomethingToDo")]
    [ApiExplorerSettings(GroupName = "v1")]
    public IActionResult Get(string whatToDo)
    {
        return ConvertToJsonObject("You wanted me to {whatToDo}, there I did something");
    }

    /// <summary>
    ///     If you want to post something for it to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpPost("PostSomething")]
    [ApiExplorerSettings(GroupName = "v1")]
    public IActionResult Post(string whatToDo)
    {
        return ConvertToJsonObject("This was passed to me {whatToDo}, there I did something");
    }

    [HttpOptions]
    public IActionResult Options()
    {
        Response.Headers.Add("Allow", "GET, POST, OPTIONS");
        return Ok();
    }
}