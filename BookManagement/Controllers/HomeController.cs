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

        public async Task<IActionResult> Detail(int bookId)
        {
            return View();
        }
        public async Task<IActionResult> Search(int pageIndex, string? keyword, int? categoryId, int? sortType)
        {
            return View();
        }
    }
}