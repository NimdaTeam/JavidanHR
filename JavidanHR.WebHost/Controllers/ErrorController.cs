using Microsoft.AspNetCore.Mvc;

namespace JavidanHR.WebHost.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/Handle")]
        public IActionResult Handle(int code)
        {
            return View(code);
        }
    }
}
