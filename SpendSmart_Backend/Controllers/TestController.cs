using Microsoft.AspNetCore.Mvc;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // GET: api/Test
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API is working!");
        }

        // POST: api/Test
        [HttpPost]
        public IActionResult CreateAdmin([FromBody] dynamic data)
        {
            // Simple test without database
            return Ok(new { 
                message = "Test successful!", 
                receivedData = data 
            });
        }
    }
}
