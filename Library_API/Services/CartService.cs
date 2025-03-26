using BookStore_API.DataAccess;
using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace BookStore_API.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IMapperService _mapperService;
        private readonly BookStoreContext _context;

        public CartService(IRepository<Cart> cartRepository, IMapperService mapperService, BookStoreContext context)
        {
            _cartRepository = cartRepository;
            _mapperService = mapperService;
            _context = context;
        }

        public async Task<IEnumerable<Cart>> GetCartByUserID(int Id)
        {
            return await _context.Carts.Where(c => c.UserID == Id).Include(c => c.Book).ToListAsync();
        }

        public async Task<Cart> AddToCart(int bookId, int userId, int quantity)
        {
            if (bookId <= 0 || userId <= 0 || quantity < 0)
                throw new ArgumentException("Thông tin giỏ hàng không hợp lệ.");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                throw new ArgumentException("Sách không tồn tại");

            var existCart = await _context.Carts.FirstOrDefaultAsync(c => c.BookID == bookId && c.UserID == userId);

            int totalQuantity = (existCart?.Quantity ?? 0) + quantity;
            if (book.Stock < totalQuantity)
                throw new ArgumentException("Số lượng sách trong kho không đủ.");

            if (existCart != null)
            {
                existCart.Quantity = totalQuantity;
                existCart.TotalPrice = totalQuantity * book.Price;
                _context.Carts.Update(existCart);
            }
            else
            {
                var newCart = new Cart
                {
                    UserID = userId,
                    BookID = bookId,
                    Quantity = quantity,
                    TotalPrice = quantity * book.Price,
                };
                await _context.Carts.AddAsync(newCart);
            }
            await _context.SaveChangesAsync();
            return existCart;
        }

        public async Task<Cart> Upsert(int bookId, int userId, int quantity)
        {
            if (bookId <= 0 || userId <= 0 || quantity < 0) 
                throw new ArgumentException("Thông tin giỏ hàng không hợp lệ.");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                throw new ArgumentException("Sách không tồn tại");

            var existCart = await _context.Carts.FirstOrDefaultAsync(c => c.BookID == bookId && c.UserID == userId);
            if (existCart != null)
            {
                if (quantity == 0)
                {
                    _context.Carts.Remove(existCart);
                    await _context.SaveChangesAsync();
                    return null;
                }
                existCart.Quantity = quantity;
                existCart.TotalPrice = quantity * book.Price;
                _context.Carts.Update(existCart);
            }
            await _context.SaveChangesAsync();
            return existCart;
        }

        public async Task<Cart> DeleteCart(int bookId, int userId)
        {
            var delete = await _context.Carts.FirstOrDefaultAsync(c => c.BookID == bookId && c.UserID == userId);
            if (delete == null)
            {
                return null;
            }
            _context.Carts.Remove(delete);
            await _context.SaveChangesAsync();
            return delete;
        }

        public async Task<bool> DeleteCartByUser(int userId)
        {
            // Assuming you have access to the cart repository
            var userCarts = (await _cartRepository.GetAll()).Where(c => c.UserID == userId).ToList();

            if (!userCarts.Any())
            {
                return false;
            }

            foreach (var cart in userCarts)
            {
                await _cartRepository.Delete(cart);
            }

            return true;
        }
    }
}