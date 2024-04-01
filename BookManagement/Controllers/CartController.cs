using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace BookManagement.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserConfig _userConfig;
        private readonly ICartService _cartService;
        private readonly IBaseService<Book> _bookService;
        private readonly IBaseService<Order> _orderService;
        private readonly IBaseService<Voucher> _voucherService;
        private readonly IBaseService<OrderDetail> _orderDetailService;

        public CartController(IUserConfig userConfig,
            IBaseService<OrderDetail> orderDetailService,
            IBaseService<Voucher> voucherService,
            IBaseService<Order> orderService,
            IBaseService<Book> bookService,
            ICartService cartService,
            IMapper mapper)
        {
            _mapper = mapper;
            _userConfig = userConfig;
            _cartService = cartService;
            _bookService = bookService;
            _orderService = orderService;
            _voucherService = voucherService;
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userConfig.GetUserId();

            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var cartList = await _cartService.GetList(x => x.UserId == userId);

            var model = await GetCartModel(cartList);

            ViewBag.CartCount = cartList.Count;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string voucherCode)
        {
            var userId = _userConfig.GetUserId();
            var cartList = await _cartService.GetList(x => x.UserId == userId);
            ViewBag.CartCount = cartList?.Count ?? 0;

            var model = await GetCartModel(cartList);

            if (string.IsNullOrEmpty(voucherCode)){
                ViewBag.ToastType = Constants.Error;
                ViewBag.ToastMessage = "Vui lòng nhập mã.";
            }
            else {
                var voucher = await _voucherService.Get(x => x.IsActive && x.VoucherCode.Trim().ToLower().Equals(voucherCode.Trim().ToLower()));

                if (voucher == null)
                {
                    ViewBag.ToastType = Constants.Error;
                    ViewBag.ToastMessage = "Mã giảm giá không khả dụng.";
                }
                else
                {
                    if (voucher.Quantity <= voucher.UsedNumber)
                    {
                        ViewBag.ToastType = Constants.Error;
                        ViewBag.ToastMessage = "Mã giảm giá đã sử dụng hết, vui lòng chọn mã khác.";
                    }
                    else if (voucher.MinAmount > model.TotalMoney)
                    {
                        ViewBag.ToastType = Constants.Error;
                        ViewBag.ToastMessage = $"Chưa đạt giá trị đơn hàng tối thiểu {voucher.MinAmount.ToString("#,##0")} đ";
                    }
                    else
                    {
                        ViewBag.ToastType = Constants.Success;
                        ViewBag.ToastMessage = "Đã áp mã giảm giá.";

                        model.VoucherId = voucher.Id;
                        model.VoucherCode = voucher.VoucherCode;
                        model.Discount = voucher.Discount;
                    }
                }
            }

            return View(model);
        }

        private async Task<CartModel> GetCartModel(List<Cart> cartList)
        {
            var model = new CartModel();

            if (cartList != null && cartList.Any())
            {
                var books = await _bookService.GetList(x => cartList.Select(x => x.BookId).Contains(x.Id));

                var joinBook = from c in cartList
                               join b in books on c.BookId equals b.Id
                               select new CartItemModel
                               {
                                   Id = c.Id,
                                   UserId = c.UserId,
                                   BookId = c.BookId,
                                   Quantity = c.Quantity,
                                   BookImage = b.BookImage,
                                   BookName = b.BookName,
                                   MaxQuantity = b.Quantity,
                                   Price = (b.PriceDiscount != null && b.PriceDiscount != 0 ? (int)b.PriceDiscount : b.Price),
                                   PriceOriginal = (b.PriceDiscount != null && b.PriceDiscount != 0 ? b.Price : null)
                               };

                model.CartItems = joinBook.ToList();
                model.ShipCost = 30000;
            }

            return model;
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmOrder(int? voucherId)
        {
            var userId = _userConfig.GetUserId();
            var cartList = await _cartService.GetList(x => x.UserId == userId);
            ViewBag.CartCount = cartList?.Count ?? 0;

            var cartModel = await GetCartModel(cartList);

            if (voucherId != null && voucherId > 0)
            {
                var voucher = await _voucherService.GetEntityById(voucherId ?? 0);

                cartModel.VoucherCode = voucher.VoucherCode;
                cartModel.Discount = voucher.Discount;
            }

            ViewBag.CartInfo = cartModel;

            var model = new CartConfirmModel()
            {
                VoucherId = voucherId,
                ShipCost = cartModel.ShipCost,
                Discount = cartModel.Discount,
                TotalMoney = cartModel.TotalMoney,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(CartConfirmModel model)
        {
            var userId = _userConfig.GetUserId();
            var cartList = await _cartService.GetList(x => x.UserId == userId);

            if (ModelState.IsValid)
            {
                if (cartList == null || !cartList.Any())
                {
                    TempData["ToastMessage"] = "Không có sản phẩm trong giỏ hàng.";
                    TempData["ToastType"] = Constants.Error;

                    return RedirectToAction("Index", "Cart");
                }
                else
                {
                    await _cartService.CreateNewOrder(userId, model);
                    return RedirectToAction("Waiting", "Cart");
                }
            }
            
            ViewBag.CartCount = cartList?.Count ?? 0;

            var cartModel = await GetCartModel(cartList);

            if (model.VoucherId != null && model.VoucherId > 0)
            {
                var voucher = await _voucherService.GetEntityById(model.VoucherId ?? 0);

                cartModel.VoucherCode = voucher.VoucherCode;
                cartModel.Discount = voucher.Discount;
            }

            ViewBag.CartInfo = cartModel;

            return View(model);
        }

        public async Task<IActionResult> Waiting(int? pageIndex)
        {
            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Waiting, pageIndex, userId);

            return View(pagingResult);
        }
        public async Task<IActionResult> Delivering(int? pageIndex)
        {
            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Shipping, pageIndex, userId);

            return View(pagingResult);
        }
        public async Task<IActionResult> OrderComplete(int? pageIndex)
        {
            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Complete, pageIndex, userId);

            return View(pagingResult);
        }
        public async Task<IActionResult> OrderCancel(int? pageIndex)
        {
            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Cancel, pageIndex, userId);

            return View(pagingResult);
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int bookId, int? quantity)
        {
            var userId = _userConfig.GetUserId();
            var book = await _bookService.GetEntityById(bookId);
            var bookUser = await _cartService.Get(x => x.UserId == userId && x.BookId == bookId);

            if (bookUser != null)
            {
                bookUser.Quantity += ((quantity != null && quantity > 0) ? quantity.Value : 1);
                bookUser.Quantity = bookUser.Quantity > book.Quantity ? book.Quantity : bookUser.Quantity;

                await _cartService.Update(bookUser);
            }
            else
            {
                var newCart = new Cart()
                {
                    BookId = bookId,
                    UserId = userId,
                    Quantity = (quantity != null && quantity > 0) ? quantity.Value : 1
                };

                newCart.Quantity = newCart.Quantity > book.Quantity ? book.Quantity : newCart.Quantity;

                await _cartService.Insert(newCart);
            }

            TempData["ToastMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
            TempData["ToastType"] = Constants.Success;

            return RedirectToAction("Index");
        }

        [HttpPut]
        public async Task<IActionResult> ChangeQuantity(int id, int quantity)
        {
            var redirectUrl = Url.Action("Index", "Cart");
            var cart = await _cartService.GetEntityById(id);

            cart.Quantity = quantity;
            await _cartService.Update(cart);

            return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveBook(int id)
        {
            var redirectUrl = Url.Action("Index", "Cart");

            await _cartService.Delete(id);

            return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
        }
    }
}
