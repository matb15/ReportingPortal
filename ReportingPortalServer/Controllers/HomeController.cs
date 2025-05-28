using Microsoft.AspNetCore.Mvc;
using ReportingPortalServer.Services.AppwriteIO;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus(IAppwriteClient appwriteClient)
        {
            try
            {
                var bucketId = appwriteClient.CreateBucketAsync("Files").Result;
                if (string.IsNullOrEmpty(bucketId))
                {
                    return StatusCode(500, new { status = "API is not running properly." });
                }

                Console.WriteLine($"Bucket created with ID: {bucketId}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "API is not running properly.", error = ex.Message });
            }
            {
                return Ok(new { status = "API is running." });
            }
        }
    }
}
