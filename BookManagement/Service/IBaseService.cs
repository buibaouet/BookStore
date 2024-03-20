﻿using BookManagement.Models;
using System.Linq.Expressions;

namespace BookManagement.Service
{
    public interface IBaseService<T>
    {
        List<T> GetAll();
        Task<T> GetEntityById(int id);
        Task<T> Get(Expression<Func<T, bool>> expresstion);
        Task<List<T>> GetList(Expression<Func<T, bool>> expresstion);
        Task<bool> Exist(Expression<Func<T, bool>> expresstion);
        Task Insert(T entity);
        Task InsertMulti(List<T> entities);
        Task Update(T entity);
        Task Delete(int id);
    }
}
