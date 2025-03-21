using BookStore_API.Domain.DTO;
using BookStore_API.Models;

namespace BookStore_API.Services.Interface
{
    public interface IUserService
    {
        Task<User> Register(UserDTO userDTO);
        Task<string?> Login(string username, string password);
    }
}