using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloWord.Controllers
{
    public class HeatherController: AbstractController
    {
        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}
