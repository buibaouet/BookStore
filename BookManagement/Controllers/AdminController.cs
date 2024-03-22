using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagement.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class AdminController : Controller
    {

        public AdminController()
        {
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult CategoryManagement()
        {
            return View();
        }
        public IActionResult BookManagement()
        {
            return View();
        }
        public IActionResult BookDetail()
        {
            return View();
        }
        public IActionResult WaitingDelivery()
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
        public IActionResult VoucherManagement()
        {
            return View();
        }
    }
}
