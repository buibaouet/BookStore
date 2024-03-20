﻿using AutoMapper;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;

namespace BookManagement.Service
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly IBaseService<User> _userService;

        public AuthService(IMapper mapper, IBaseService<User> userService)
        {
            _mapper = mapper;
            _userService = userService;
        }

        /// <summary>
        /// Insert user mới đăng ký
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertUser(RegisterModel model)
        {
            var user = _mapper.Map<User>(model);

            user.Password = await HashPassword(model.Password);
            user.IsAdmin = false;

            await _userService.Insert(user);
        }

        public async Task<User> AuthenticationUser(UserModel model)
        {
            var user = await _userService.Get(x => x.UserName.Equals(model.UserName));

            if (user != null)
            {
                if(await ValidateHashPassword(model.Password, user.Password))
                {
                    return user;
                }
            }

            return null;
        }

        /// <summary>
        /// Decode password
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<string> HashPassword(string value)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(value, BCrypt.Net.BCrypt.GenerateSalt(12)));
        }

        /// <summary>
        /// Verify password
        /// </summary>
        /// <param name="value">Password người dùng nhập</param>
        /// <param name="hash">Password đã decode</param>
        /// <returns></returns>
        public Task<bool> ValidateHashPassword(string value, string hash)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.Verify(value, hash));
        }

    }
}