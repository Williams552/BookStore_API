using Library_API.Domain.DTO;
using Library_API.Models;
using Library_API.Services.Interface;

namespace Library_API.Services
{
    public class AuthorServices : IAuthorServices
    {
        public Author AuthorDTOtoAuthor(AuthorCreateDTO authorDTO)
        {
            var author = new Author
            {
                Fullname = authorDTO.Fullname,
                Biography = authorDTO.Biography,
                ImageURL = authorDTO.ImageURL,
                CreateAt = DateTime.Now
            };
            return author;
        }
    }
}
