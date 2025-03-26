using BookStore_API.Models;
namespace BookStore_API.Services.Interface
{
    public interface IWishService
    {
        Task<IEnumerable<WishList>> GetUserByID(int Id);
        Task<WishList?> AddWish(int bookId, int userId);
        Task<WishList> Delete(int bookId, int userId);
    }
}
