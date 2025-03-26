using BookStore_API.DataAccess;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace BookStore_API.Services
{
    public class WishService : IWishService
    {
        private readonly IRepository<WishList> _wishLish;
        private readonly IMapperService _mapperService;
        private readonly BookStoreContext _context;

        public WishService(IRepository<WishList> wishLish, IMapperService mapperService, BookStoreContext context)
        {
            _wishLish = wishLish;
            _mapperService = mapperService;
            _context = context;
        }

        public async Task<IEnumerable<WishList>> GetUserByID(int Id)
        {
            return await _context.WishLists.Where(c => c.UserID == Id).Include(c => c.Book).ToListAsync();
        }

        public async Task<WishList?> AddWish(int bookId, int userId)
        {
            if (bookId <= 0 || userId <= 0)
                throw new ArgumentException("Thông tin không hợp lệ");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                throw new ArgumentException("Sách không tồn tại");

            var exist = await _context.WishLists.FirstOrDefaultAsync(c => c.BookID == bookId && c.UserID == userId);
            if (exist != null)
            {
                return exist;
            }

            var newItem = new WishList
            {
                UserID = userId,
                BookID = bookId,
            };

            await _context.WishLists.AddAsync(newItem);
            await _context.SaveChangesAsync();
            return newItem;
        }


        public async Task<WishList> Delete(int bookId, int userId)
        {
            var delete = await _context.WishLists.FirstOrDefaultAsync(c => c.BookID == bookId && c.UserID == userId);
            if (delete == null)
            {
                return null;
            }
            _context.WishLists.Remove(delete);
            await _context.SaveChangesAsync();
            return delete;
        }
    }
}
