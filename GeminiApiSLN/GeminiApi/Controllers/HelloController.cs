
using Microsoft.AspNetCore.Mvc;
using System;

namespace GeminiApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet("{name}")]
        public string Get(string name)
        {
            return "Hello " + name;
        }

        [HttpGet("time")]
        public string GetTime()
        {
            return DateTime.Now.ToString();
        }
    }
}
