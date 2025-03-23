using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using System.Threading.Tasks;

namespace BookStore_API.Services.Interface
{
    public interface ICartService
    {
        Task<IEnumerable<Cart>> GetCartByUserID(int Id);
        Task<Cart?> Upsert(int bookId, int userId, int quantity);
        Task<Cart> DeleteCart(int bookId, int userId);

    }
}
