using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using X.PagedList;

namespace BookManagement.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBaseService<Category> _categoryService;
        private readonly IBaseService<Voucher> _voucherService;
        private readonly IBaseService<Book> _bookService;
        private readonly IMapper _mapper;

        public AdminController(IBaseService<Category> categoryService,
            IWebHostEnvironment environment,
            IBaseService<Voucher> voucherService,
            IBaseService<Book> bookService,
            IMapper mapper)
        {
            _hostingEnvironment = environment;
            _categoryService = categoryService;
            _voucherService = voucherService;
            _bookService = bookService;
            _mapper = mapper;
        }

        //GET: /Admin/Dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        #region Category Management
        //GET: /Admin/CategoryManagement
        [HttpGet]
        public async Task<IActionResult> CategoryManagement(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var categoryAll = _categoryService.GetAll();

            // Join lấy thông tin số lượng sách
            var categories = _mapper.Map<List<CategoryModel>>(categoryAll);

            foreach (var category in categories)
            {
                category.TotalBook = await _bookService.Count(x => x.CategoryId == category.Id);
            }

            var pagingResult = new PagingModel<CategoryModel>()
            {
                TotalRecord = categories.Count(),
                DataPaging = categories.OrderBy(x => x.CategoryCode).ToPagedList(pageIndex ?? 1, 10),
            };

            return View(pagingResult);
        }

        //Get: /Admin/CategoryById
        [HttpGet]
        public async Task<JsonResult> CategoryById(int id)
        {
            var entity = await _categoryService.GetEntityById(id);

            return Json(entity);
        }

        //Get: /Admin/ExistCategoryCode
        [HttpGet]
        public async Task<JsonResult> ExistCategoryCode(string code, int? id)
        {
            var exist = await _categoryService.Exist(x => x.CategoryCode.Trim().ToLower().Equals(code.Trim().ToLower())
                                                            && (id != null && id != 0 ? x.Id != id : x.Id > 0));

            return Json(exist);
        }

        //Post: /Admin/InsertCategory
        [HttpPost]
        public async Task<IActionResult> InsertCategory([FromBody] Category model)
        {
            var redirectUrl = Url.Action("CategoryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _categoryService.Insert(model);

                TempData["ToastMessage"] = "Thêm mới danh mục thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Put: /Admin/UpdateCategory
        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] Category model)
        {
            var redirectUrl = Url.Action("CategoryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _categoryService.Update(model);

                TempData["ToastMessage"] = "Cập nhật danh mục thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Delete: /Admin/DeleteCategory
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var redirectUrl = Url.Action("CategoryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _categoryService.Delete(id);

                TempData["ToastMessage"] = "Xóa danh mục thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }
        #endregion

        #region Book Management
        //GET: /Admin/BookManagement
        [HttpGet]
        public async Task<IActionResult> BookManagement(int? pageIndex, string? keyword, int? categoryId)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var result = new BookPagingModel()
            {
                CategoryId = categoryId,
                Keyword = keyword,
                Paging = new PagingModel<Book>()
            };

            var books = await _bookService.GetList(x => (string.IsNullOrEmpty(keyword) ? x.Id > 0 : x.BookName.ToLower().Contains(keyword.ToLower().Trim()))
                && (categoryId != null && categoryId != 0 ? x.CategoryId == categoryId : x.Id > 0));

            result.Paging = new PagingModel<Book>()
            {
                TotalRecord = books.Count(),
                DataPaging = books.OrderBy(x => x.BookName).ToPagedList(pageIndex ?? 1, 10),
            };

            var cate = _categoryService.GetAll();
            cate.Insert(0, new Category()
            {
                CategoryName = "Chọn danh mục"
            });

            SelectList cateList = new SelectList(cate, "Id", "CategoryName");
            // Set vào ViewBag
            ViewBag.CategoryList = cateList;

            return View(result);
        }

        //GET: /Admin/BookDetail
        [HttpGet]
        public async Task<IActionResult> BookDetail(int? id)
        {
            if (id != null)
            {
                ViewData["HeaderTitle"] = "Sửa thông tin sách";
            }
            else
            {
                ViewData["HeaderTitle"] = "Thêm sách mới";
            }

            var cate = _categoryService.GetAll();
            cate.Insert(0, new Category()
            {
                CategoryName = "Chọn danh mục"
            });

            SelectList cateList = new SelectList(cate, "Id", "CategoryName");
            // Set vào ViewBag
            ViewBag.CategoryList = cateList;

            var book = (await _bookService.GetEntityById(id ?? 0)) ?? new Book();

            return View(book);
        }

        //Post: /Admin/BookDetail
        [HttpPost]
        public async Task<IActionResult> BookDetail(Book model)
        {
            var cate = _categoryService.GetAll();
            cate.Insert(0, new Category()
            {
                CategoryName = "Chọn danh mục"
            });

            SelectList cateList = new SelectList(cate, "Id", "CategoryName");
            // Set vào ViewBag
            ViewBag.CategoryList = cateList;

            bool isValid = true;

            if (ModelState.IsValid)
            {
                if (model.CategoryId == 0)
                {
                    ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục");
                    isValid = false;
                }

                if (model.PriceDiscount > 0 && model.PriceDiscount >= model.Price)
                {
                    ModelState.AddModelError("PriceDiscount", "Giá khuyến mại phải nhỏ hơn giá bán");
                    isValid = false;
                }

                var bookExist = await _bookService.Exist(x => x.BookCode.ToLower().Trim().Equals(model.BookCode.ToLower().Trim()) && model.Id != x.Id);
                if (bookExist)
                {
                    ModelState.AddModelError("BookCode", "Mã sách đã tồn tại");
                    isValid = false;
                }

                if (!isValid)
                {
                    return View(model);
                }

                if (model.Id != 0)
                {
                    await _bookService.Update(model);

                    TempData["ToastMessage"] = "Cập nhật thông tin sách thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("BookManagement");
                }
                else
                {
                    await _bookService.Insert(model);

                    TempData["ToastMessage"] = "Thêm sách mới thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("BookManagement");
                }
            }

            return View(model);
        }

        //Delete: /Admin/DeleteBook
        [HttpDelete]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var redirectUrl = Url.Action("BookManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _bookService.Delete(id);

                TempData["ToastMessage"] = "Xóa sách thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        [HttpPost]
        public IActionResult UploadImage([FromBody] UploadModel upload)
        {
            if (!string.IsNullOrEmpty(upload.BookImageUri))
            {
                var imgName = Guid.NewGuid().ToString();

                using (Image image = Base64ToImage(upload.BookImageUri))
                {
                    string bookDetailImageUri = "\\wwwroot\\uploads\\" + imgName + ".jpg";
                    string strFileName = Directory.GetCurrentDirectory() + bookDetailImageUri;
                    image.Save(strFileName, ImageFormat.Jpeg);

                    if (System.IO.File.Exists(strFileName))
                    {
                        return Json(new { Success = true, FileName = imgName + ".jpg" });
                    }
                    else
                    {
                        return Json(new { Success = false });
                    }
                }
            }
            else
            {
                return Json(new { Success = false });
            }
        }

        /// <summary>
        /// Convert từ Base64 sang Image
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        private Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            Bitmap tempBmp;
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                using (Image image = Image.FromStream(ms, true))
                {
                    //Create another object image for dispose old image handler
                    tempBmp = new Bitmap(image.Width, image.Height);
                    Graphics g = Graphics.FromImage(tempBmp);
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }
            return tempBmp;
        }
        #endregion

        #region Cart Management
        //GET: /Admin/WaitingDelivery
        [HttpGet]
        public IActionResult WaitingDelivery()
        {
            return View();
        }
        //GET: /Admin/Delivering
        [HttpGet]
        public IActionResult Delivering()
        {
            return View();
        }
        //GET: /Admin/OrderComplete
        [HttpGet]
        public IActionResult OrderComplete()
        {
            return View();
        }
        //GET: /Admin/OrderCancel
        [HttpGet]
        public IActionResult OrderCancel()
        {
            return View();
        }
        #endregion

        #region Voucher Management
        //GET: /Admin/VoucherManagement
        [HttpGet]
        public IActionResult VoucherManagement(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var vouchers = _voucherService.GetAll();

            var pagingResult = new PagingModel<Voucher>()
            {
                TotalRecord = vouchers.Count(),
                DataPaging = vouchers.OrderBy(x => x.VoucherCode).ToPagedList(pageIndex ?? 1, 10),
            };

            return View(pagingResult);
        }

        //Get: /Admin/VoucherById
        [HttpGet]
        public async Task<JsonResult> VoucherById(int id)
        {
            var entity = await _voucherService.GetEntityById(id);

            return Json(entity);
        }

        //Get: /Admin/ExistVoucherCode
        [HttpGet]
        public async Task<JsonResult> ExistVoucherCode(string code, int? id)
        {
            var exist = await _voucherService.Exist(x => x.VoucherCode.Trim().ToLower().Equals(code.Trim().ToLower())
                                                            && (id != null && id != 0 ? x.Id != id : x.Id > 0));

            return Json(exist);
        }

        //Post: /Admin/InsertVoucher
        [HttpPost]
        public async Task<IActionResult> InsertVoucher([FromBody] Voucher model)
        {
            var redirectUrl = Url.Action("VoucherManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _voucherService.Insert(model);

                TempData["ToastMessage"] = "Thêm mã khuyến mại thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Put: /Admin/UpdateVoucher
        [HttpPut]
        public async Task<IActionResult> UpdateVoucher([FromBody] Voucher model)
        {
            var redirectUrl = Url.Action("VoucherManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _voucherService.Update(model);

                TempData["ToastMessage"] = "Cập nhật mã khuyến mại thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Delete: /Admin/DeleteVoucher
        [HttpDelete]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var redirectUrl = Url.Action("VoucherManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _voucherService.Delete(id);

                TempData["ToastMessage"] = "Xóa mã khuyến mại thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }
        #endregion
    }
}
