using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using System.Linq;
using System.Linq.Expressions;

namespace BookManagement.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        private readonly IMapper _mapper;
        private readonly IBaseService<Category> _cateService;

        public BookService(IGenericRepository<Book> baseRepo,
            ILogger<Book> logger,
            IBaseService<Category> cateService,
            IMapper mapper) : base(baseRepo, logger)
        {
            _mapper = mapper;
            _cateService = cateService;
        }

        public List<Book> GetBookActiveInCategoryActive(Expression<Func<Book, bool>> expresstion)
        {
            var cateActive = _cateService.GetDbSet().Where(x => x.IsActive).ToList();

            var bookActive = from b in _baseRepo.GetDbSet().Where(expresstion).ToList()
                             join c in cateActive
                             on b.CategoryId equals c.Id
                             where b.IsActive && b.Quantity > 0
                             select b;

            return  bookActive.ToList();
        }
    }
}
