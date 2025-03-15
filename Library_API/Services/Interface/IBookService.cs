using BookStore_API.Domain.DTO;
using BookStore_API.Models;

namespace BookStore_API.Services.Interface
{
    public interface IBookService
    {
        Task<BookDTO> CreateBook(BookDTO bookDTO);
    }
}