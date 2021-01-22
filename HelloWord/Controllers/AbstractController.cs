using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloWord.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public abstract class AbstractController : ControllerBase
    {
    }
}
