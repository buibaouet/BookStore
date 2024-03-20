using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookManagement.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBaseService<User> _userService;

        public HomeController(ILogger<HomeController> logger, IBaseService<User> userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Products = await _userService.GetEntityById(1);
            return View();
        }

        public async Task<IActionResult> Detail()
        {
            return View();
        }
        public async Task<IActionResult> Search()
        {
            return View();
        }
        public async Task<IActionResult> Category()
        {
            return View();
        }
    }
}