using Microsoft.AspNetCore.Mvc;

namespace SiteMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {

        [HttpGet]
        public ActionResult<string> Get()
        {
            return "Ok";
        }
    }
}