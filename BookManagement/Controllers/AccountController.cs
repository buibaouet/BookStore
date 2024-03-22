using BookManagement.Constant;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

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
        public IActionResult Login(string? returnUrl = null)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserModel model, string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _authService.AuthenticationUser(model);
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(user)),
                        new Claim(ClaimTypes.Role, user.IsAdmin ? Role.Admin : Role.Default),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = model.RememberMe,
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

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
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
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
