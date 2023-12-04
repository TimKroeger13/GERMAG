using Microsoft.AspNetCore.Mvc;

namespace GERMAG.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloWorldController :  ControllerBase
    {
        [HttpGet("greet")]
        public string Get()
        {
            return "Hello";
        }
    }
}
