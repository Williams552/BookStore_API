using Books_API.Domain.DTO;
using Books_API.Models;

namespace Books_API.Services.Interface
{
    public interface IBookService
    {
        Task<Book> CreateBook(BookDTO bookDTO);
    }
}