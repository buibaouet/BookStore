using BookManagement.Constant;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;

namespace BookManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IBaseService<User> _userService;

        public AccountController(
            IAuthService authService,
            IBaseService<User> userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // Get: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _authService.AuthenticationUser(model);
                if (user != null)
                {
                    HttpContext.Session.SetString("_userConfig", JsonConvert.SerializeObject(user));
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ViewBag.ToastMessage = "Tài khoản hoặc mật khẩu không chính xác.";
                    ViewBag.ToastType = Constants.Error;

                    return View(model);
                }
            }
            return View(model);
        }

        // Get: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userService.Exist(x => x.UserName.Equals(model.UserName));
                if (userExist)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }
                else
                {
                    await _authService.InsertUser(model);

                    TempData["ToastMessage"] = "Đăng ký tài khoản thành công.";
                    TempData["ToastType"] = Constants.Success;

                    return RedirectToAction("Login");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("_userConfig");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
