using Microsoft.AspNetCore.Mvc;

namespace JavidanHR.WebHost.Controllers.PayrollSystem.Contract
{
    public class ContractController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
