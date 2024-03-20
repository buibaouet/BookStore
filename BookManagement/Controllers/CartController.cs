using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagement.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Waiting()
        {
            return View();
        }
        public IActionResult Delivering()
        {
            return View();
        }
        public IActionResult OrderComplete()
        {
            return View();
        }
        public IActionResult OrderCancel()
        {
            return View();
        }
    }
}
