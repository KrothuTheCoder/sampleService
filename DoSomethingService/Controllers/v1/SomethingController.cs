using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DoSomethingService.Controllers.v1;

/// <summary>
/// This is an api to return some data in a made up senario
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[EnableCors("myAllowSpecificOrigins")]
public class SomethingController : ControllerBase
{
    private readonly ILogger<SomethingController> _logger;
    private readonly TelemetryClient _telemetryClient;


    /// <summary>
    /// Setting up the code so it can do things
    /// </summary>
    /// <param name="logger">The logger so we know if things have been done</param>
    public SomethingController(ILogger<SomethingController> logger, TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    /// <summary>
    /// If you need to do something then it will
    /// </summary>
    /// <returns>What happened</returns>
    [HttpGet("DoSomething")]
    [ApiExplorerSettings(GroupName ="v1")]
    public IActionResult Get()
    {
        var doSomethingDictionary = new Dictionary<string, string>();
         doSomethingDictionary.Add("EventName", "Api Call");
        _telemetryClient.TrackEvent("DoSomething", doSomethingDictionary);
        return ConvertToJsonObject("You wanted me to do something, there I did something");
   
    }


    /// <summary>
    /// If you need to do something then it will
    /// </summary>
    /// <returns>What happened</returns>
    private ContentResult ConvertToJsonObject(string input)
    {
        //Response.Headers.Add("Access-Control-Allow-Origin", "https://web.hakabo.com");
        Response.Headers.Add("x-peters-test", "https://web.hakabo.com");
        var jsonString = "{\"key\":\""+input+"\"}";
        //string jsonString = $"{\"key\":\"{input}\"}"; 
        return new ContentResult
            {
                Content = jsonString,
                ContentType = "application/json",
                StatusCode = 200   // You can set the status code as needed
            };
    }

    /// <summary>
    /// If you need to do something then, just tell it
    /// what you want to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpGet("WhatSomethingToDo")]
    [ApiExplorerSettings(GroupName ="v1")]
    public IActionResult Get(string whatToDo)
    {
        return ConvertToJsonObject("You wanted me to {whatToDo}, there I did something");
    }
   
    /// <summary>
    /// If you want to post something for it to do
    /// </summary>
    /// <param name="whatToDo">The thing you want it to do</param>
    /// <returns>What happened</returns>
    [HttpPost("PostSomething")]
    [ApiExplorerSettings(GroupName ="v1")]
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