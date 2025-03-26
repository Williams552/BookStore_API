using BookStore_API.Domain.DTO;
using BookStore_API.DTOs;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Net;
using BookStore_API.DTOs.User;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRepository<User> _userRepository;
        private readonly IMapperService _mapperService;
        private readonly IUserService _userService;

        public UserController(IRepository<User> userRepository, IMapperService mapperService, IUserService userService)
        {
            _userRepository = userRepository;
            _mapperService = mapperService;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var existingUser = await _userRepository.GetFirstByCondition(u => u.Email == userDTO.Email || u.Username == userDTO.Username);
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "Email hoặc Username đã tồn tại." });
            }

            var user = new User
            {
                Username = userDTO.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                Email = userDTO.Email,
                Phone = userDTO.Phone,
                CreateAt = DateOnly.FromDateTime(DateTime.Now),
                IsDelete = false,
                Role = 3 // Chưa active
            };

            await _userRepository.Add(user);
            return Ok(new { success = true, message = "Đăng ký thành công, vui lòng xác nhận OTP.", email = user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var token = await _userService.Login(username, password);
            if (token == null)
            {
                return Unauthorized("Invalid username or password");
            }
            return Ok(new { Token = token });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await Task.Run(() => _userRepository.GetAll());
            if (users == null || !users.Any())
            {
                return NotFound("No users found.");
            }
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await Task.Run(() => _userRepository.GetById(id));
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDTO userDTO)
        {
            if (id != userDTO.UserID)
            {
                return BadRequest("User ID mismatch.");
            }

            var existingUser = await Task.Run(() => _userRepository.GetById(id));
            if (existingUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            var user = _mapperService.MapToDto<UserDTO, User>(userDTO);
            if (string.IsNullOrEmpty(user.Username)) user.Username = existingUser.Username;
            if (string.IsNullOrEmpty(user.ImageUrl)) user.ImageUrl = existingUser.ImageUrl;
            user.CreateAt = existingUser.CreateAt;
            user.IsDelete = existingUser.IsDelete;
            user.Role = existingUser.Role;

            await Task.Run(() => _userRepository.Update(user));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await Task.Run(() => _userRepository.GetById(id));
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            await Task.Run(() => _userRepository.Delete(user));
            return NoContent();
        }

        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetFirstByCondition(u => u.Email == email);
            if (user == null)
            {
                return NotFound($"User with email {email} not found.");
            }
            return Ok(user);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest(new { Message = "Email và OTP là bắt buộc." });
            }

            var user = await _userRepository.GetFirstByCondition(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { Message = "Email không tồn tại trong hệ thống." });
            }

            if (user.OTP == null || user.OTP.ToString() != request.Otp)
            {
                return BadRequest(new { Message = "OTP không hợp lệ." });
            }

            if (user.TimeOtp == null || (DateTime.UtcNow - user.TimeOtp.Value).TotalMinutes > 5)
            {
                return BadRequest(new { Message = "OTP đã hết hạn." });
            }

            user.IsActive = true; // Kích hoạt user
            user.OTP = null;
            user.TimeOtp = null;
            await _userRepository.Update(user);

            return Ok(new { Message = "Xác thực OTP thành công. Đăng ký hoàn tất." });
        }

        [HttpPost("forgot-password")]
        [Consumes("application/json")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Message = "Email là bắt buộc." });
            }

            var user = await _userRepository.GetFirstByCondition(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { Message = "Email không tồn tại trong hệ thống." });
            }

            try
            {
                int otp = GenerateOTP();
                await SendOTPEmailAsync(user.Email, otp.ToString());
                user.OTP = otp;
                user.TimeOtp = DateTime.UtcNow;
                await _userRepository.Update(user);
                return Ok(new { Message = "OTP đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi gửi OTP: {ex.Message}" });
            }
        }

        [HttpPost("resend-otp")]
        [Consumes("application/json")]
        public async Task<IActionResult> ResendOtp([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Message = "Email là bắt buộc." });
            }

            var user = await _userRepository.GetFirstByCondition(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { Message = "Email không tồn tại trong hệ thống." });
            }

            try
            {
                int otp = GenerateOTP();
                await SendOTPEmailAsync(user.Email, otp.ToString());
                user.OTP = otp;
                user.TimeOtp = DateTime.UtcNow;
                await _userRepository.Update(user);
                return Ok(new { Message = "OTP mới đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi gửi OTP: {ex.Message}" });
            }
        }

        [HttpPost("reset-password")]
        [Consumes("application/json")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { Message = "Email và mật khẩu mới là bắt buộc." });
            }

            var user = await _userRepository.GetFirstByCondition(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { Message = "Email không tồn tại trong hệ thống." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.Update(user);

            return Ok(new { Message = "Đặt lại mật khẩu thành công." });
        }

        // Helper methods
        private int GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999); // OTP 6 chữ số
        }

        private async Task SendOTPEmailAsync(string email, string otp)
        {
            var fromAddress = new MailAddress("phamngocquan812@gmail.com", "Cursus");
            var toAddress = new MailAddress(email);
            const string fromPassword = "ljzi zden qcwr mcwt"; // App Password nếu dùng Gmail 2FA
            const string subject = "Your OTP for Password Reset";
            string body = $"Dear {email},\n\nYour OTP code is {otp}. It is valid for 5 minutes.\n\nBest regards,\nCursus";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            await smtp.SendMailAsync(message);
        }

        [HttpPost("save-otp")]
        public async Task<IActionResult> SaveOtp([FromBody] SaveOtpRequest request)
        {
            try
            {
                var user = await _userRepository.GetFirstByCondition(u => u.Email == request.Email);
                if (user == null)
                {
                    return NotFound(new { Message = "Email không tồn tại trong hệ thống." });
                }

                user.OTP = request.Otp;
                user.TimeOtp = DateTime.UtcNow;
                await _userRepository.Update(user);
                return Ok(new { Message = "OTP đã được lưu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi lưu OTP: {ex.Message}" });
            }
        }

        public class SaveOtpRequest
        {
            public string Email { get; set; }
            public int Otp { get; set; }
        }
    }
}