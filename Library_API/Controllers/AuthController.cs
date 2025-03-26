using BookStore_API.DataAccess;
using BookStore_API.Domain.DTO;
using BookStore_API.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BookStoreContext _context;

        public AuthController(BookStoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Xử lý đăng nhập bằng Google
        /// </summary>
        /// <param name="request">Thông tin từ Google (email, firstName, lastName)</param>
        /// <returns>Thông tin user nếu thành công</returns>
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email không hợp lệ." });
                }

                var emailClaim = request.Email;
                var firstName = request.FirstName ?? "";
                var lastName = request.LastName ?? "";

                // Tìm user trong database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);

                if (user == null)
                {
                    // Tạo user mới nếu chưa tồn tại
                    user = new BookStore_API.Models.User
                    {
                        Username = $"{firstName} {lastName}".Trim(),
                        Email = emailClaim,
                        Password = null, // Không cần mật khẩu vì dùng Google
                        FullName = $"{firstName} {lastName}".Trim(),
                        Gender = null,
                        Address = null,
                        Phone = null,
                        ImageUrl = null,
                        CreateAt = DateOnly.FromDateTime(DateTime.Now),
                        UpdateAt = null,
                        IsDelete = false,
                        Role = 3 // Role mặc định cho user thường (giả sử 3 là user)
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Tạo DTO để trả về
                var userDTO = new UserDTO
                {
                    UserID = user.UserID,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                };

                return Ok(new { success = true, user = userDTO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }
    }

    // DTO để nhận dữ liệu từ client
    
}