using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using DoSomethingService.Models;
using Microsoft.AspNetCore.Cors;

namespace DoSomethingService.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[EnableCors()]
    public class IdentityController : Controller
    {
        [HttpPost("validate")]
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> validate(){
            
            string input = null;

            // If not data came in, then return
            if (this.Request.Body == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is null", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = InputClaimsModel.Parse(input);

            //Check if the email parameter is presented
            if (string.IsNullOrEmpty(inputClaims.email))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Email is null or empty", HttpStatusCode.Conflict));
            }

            // Validate the email address 
            if (inputClaims.email.ToLower().StartsWith("test"))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Your email address can't start with 'test'", HttpStatusCode.Conflict));
            }

            if (!inputClaims.email.ToLower().EndsWith("@insightinvestment.com"))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Your email address has not been registered, please contact Bob", HttpStatusCode.Conflict));
            }

            try
            {
                return StatusCode((int)HttpStatusCode.OK, new B2CResponseModel(string.Empty, HttpStatusCode.OK)
                {
                    Role = "testRole"
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"General error (REST API): {ex.Message}", HttpStatusCode.Conflict));
            }
        }
    }

