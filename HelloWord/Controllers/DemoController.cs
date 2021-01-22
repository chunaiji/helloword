using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloWord.Controllers
{
    public class DemoController : AbstractController
    {
        private readonly IConfiguration configuration;
        public DemoController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public string HelloWord()
        {
            var value = configuration.GetValue<string>("Server");
            return $"Helloword {value} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}";
        }
    }
}
