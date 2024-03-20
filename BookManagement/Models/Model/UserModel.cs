using BookManagement.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Model
{
    public class UserModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Họ không được để trống")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên không được để trống")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu chứa tối thiểu 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-z])(?=.*[0-9])(?=.*?[!@#$%\^&*\(\)\-_+=;:'""\/\[\]{},.<>|`]).{8,32}$",
                            ErrorMessage = "Mật khẩu chứa tối thiểu 1 chữ số và 1 ký tự đặc biệt")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
