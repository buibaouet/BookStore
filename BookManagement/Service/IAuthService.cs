using BookManagement.Models.Entity;
using BookManagement.Models.Model;

namespace BookManagement.Service
{
    public interface IAuthService 
    {
        Task InsertUser(RegisterModel model);
        Task<User> AuthenticationUser(UserModel model);
    }
}
