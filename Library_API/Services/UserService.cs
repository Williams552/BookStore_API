using BookStore_API.Authentions;
using BookStore_API.DataAccess;
using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace BookStore_API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IMapperService _mapperService;
        private readonly JWTAuthService _jwtService;
        private readonly BookStoreContext _context;

        public UserService(IRepository<User> userRepository, IMapperService mapperService, JWTAuthService jwtAuthService, BookStoreContext context)
        {
            _userRepository = userRepository;
            _mapperService = mapperService;
            _jwtService = jwtAuthService;
            _context = context;
        }

        public async Task<User> Register(UserDTO userDTO)
        {
            var checkUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDTO.Username);
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
            if (checkUser != null) return null;
            var newUser = new User
            {
                Username = userDTO.Username,
                Password = hashPassword,
                Email = userDTO.Email,
                Phone = userDTO.Phone,
                Role = 2
            };
            await Task.Run(() => _userRepository.Add(newUser));
            return newUser;
        } 

        public async Task<string?> Login(string username, string password)
        {
            var checkLogin = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (checkLogin == null || !BCrypt.Net.BCrypt.Verify(password, checkLogin.Password))
            {
                return null;
            }
            return _jwtService.GenerateJwtToken(checkLogin);
        }

    }
}