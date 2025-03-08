using Library_API.Domain.DTO;
using Library_API.Models;

namespace Library_API.Services.Interface
{
    public interface IAuthorServices
    {
        public Author AuthorDTOtoAuthor(AuthorCreateDTO authorDTO);
    }
}