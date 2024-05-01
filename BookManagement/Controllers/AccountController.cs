using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Security.Claims;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IBaseService<User> _userService;
        private readonly IBaseService<Cart> _cartService;
        private readonly IConfiguration _configuration;
        private readonly IUserConfig _userConfig;

        public AccountController(
            IMapper mapper,
            IAuthService authService,
            IBaseService<User> userService,
            IBaseService<Cart> cartService,
            IUserConfig userConfig,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _userConfig = userConfig;
            _authService = authService;
            _userService = userService;
            _cartService = cartService;
            _configuration = configuration;
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

        // Get: /Account/LoginGoogle
        [HttpGet]
        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("SigninGoogle")
                });
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
                    if (user.IsDelete)
                    {
                        ViewBag.ToastMessage = "Tài khoản không khả dụng hoặc đã bị xóa. Vui lòng kiểm tra lại.";
                        ViewBag.ToastType = Constants.Error;

                        return View(model);
                    }
                    else if (!user.IsActive)
                    {
                        ViewBag.ToastMessage = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên để được hỗ trợ.";
                        ViewBag.ToastType = Constants.Error;

                        return View(model);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(user)),
                        new Claim(ClaimTypes.Role, user.RoleType == RoleEnum.Admin ? Role.Admin :
                        (user.RoleType == RoleEnum.Staff ? Role.Staff : Role.User)),
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

        public async Task<IActionResult> SigninGoogle()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsData = result.Principal.Identities.FirstOrDefault().Claims.ToList();

            // Đăng nhập google thành công 
            var email = claimsData.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = await _userService.Get(x => x.Email.ToLower().Trim().Equals(email.ToLower().Trim()));

            // Nếu chưa có user thì insert user vào database
            if (user is null)
            {
                await _authService.InsertUser(new RegisterModel
                {
                    FirstName = claimsData.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value,
                    LastName = claimsData.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value,
                    Email = email,
                    UserName = email,
                    Password = _configuration.GetSection("Password:Default").Value
                });

                user = await _userService.Get(x => x.UserName.ToLower().Trim().Equals(email.ToLower().Trim()));
            }

            if (user.IsDelete)
            {
                TempData["ToastMessage"] = "Tài khoản không khả dụng hoặc đã bị xóa. Vui lòng kiểm tra lại.";
                TempData["ToastType"] = Constants.Error;

                return RedirectToAction("Login");
            }
            else if (!user.IsActive)
            {
                TempData["ToastMessage"] = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên để được hỗ trợ.";
                TempData["ToastType"] = Constants.Error;

                return RedirectToAction("Login");
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(user)),
                    new Claim(ClaimTypes.Role, user.RoleType == RoleEnum.Admin ? Role.Admin :
                        (user.RoleType == RoleEnum.Staff ? Role.Staff : Role.User)),
                };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction(nameof(HomeController.Index), "Home");
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
                var userExist = await _userService.Exist(x => x.UserName.ToLower().Trim().Equals(model.UserName.ToLower().Trim()));
                var emailExist = await _userService.Exist(x => x.Email.ToLower().Trim().Equals(model.Email.ToLower().Trim()));
                if (userExist && emailExist)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }
                else if (userExist)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }
                else if (emailExist)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
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
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> ChangePassword(PasswordModel model)
        {
            var userId = _userConfig.GetUserId();
            // Check pass nhập vào xem có đúng không
            var user = await _userService.GetEntityById(userId);
            var validPass = await _authService.ValidateHashPassword(model.OldPassword, user.Password);

            if (!validPass)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không chính xác");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                user.Password = await _authService.HashPassword(model.NewPassword);
                await _userService.Update(user);

                TempData["ToastMessage"] = "Cập nhật mật khẩu thành công.";
                TempData["ToastType"] = Constants.Success;

                return RedirectToAction("ChangePassword");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Infomation()
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var user = await _userService.GetEntityById(userId);

            var userModel = _mapper.Map<UserInfomationModel>(user);

            // Set vào ViewBag
            ViewBag.GenderList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = 0, Name = "Chọn giới tính" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Male, Name = "Nam" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Female, Name = "Nữ" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Other, Name = "Khác" },
            }, "Value", "Name");

            return View(userModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Infomation(UserInfomationModel model)
        {
            // Set vào ViewBag
            ViewBag.GenderList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = 0, Name = "Chọn giới tính" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Male, Name = "Nam" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Female, Name = "Nữ" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Other, Name = "Khác" },
            }, "Value", "Name");

            if (ModelState.IsValid)
            {
                var userExist = await _userService.Exist(x => x.UserName.ToLower().Trim().Equals(model.UserName.ToLower().Trim()) && model.Id != x.Id);
                var emailExist = await _userService.Exist(x => x.Email.ToLower().Trim().Equals(model.Email.ToLower().Trim()) && model.Id != x.Id);
                if (userExist && emailExist)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }
                else if (userExist)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }
                else if (emailExist)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }
                else
                {
                    var user = await _userService.GetEntityById(model.Id);

                    var userModel = _mapper.Map(model, user);

                    await _userService.Update(userModel);

                    TempData["ToastMessage"] = "Cập nhật thông tin tài khoản thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("Infomation");
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
