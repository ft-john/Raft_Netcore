using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FtJohn.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost]
        [Route("Hello")]
        public ActionResult Hello(TestModel model)
        {
            if(Request.Host.ToString().ToLower().StartsWith("localhost"))
            {
                return new RedirectResult("http://localhost:6006/api/test/test2", false, false);
            }
            else
            {
                return new JsonResult(new { msg = "hello from 127.0.0.1" });
            }
        }

        [HttpPost]
        [Route("Test2")]
        public ActionResult Test2(TestModel model)
        {
            return new JsonResult(new { msg = "Test2 from 127.0.0.1" });
        }
    }

    public class TestModel
    {
        public string msg { get; set; }
    }
}