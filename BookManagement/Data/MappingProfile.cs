using AutoMapper;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;

namespace BookManagement.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, RegisterModel>();
            CreateMap<RegisterModel, User>();
        }
    }
}
