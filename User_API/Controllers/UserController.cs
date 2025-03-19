using Microsoft.AspNetCore.Mvc;
using Users_API.Domain.DTO;
using Users_API.Models;
using Users_API.Repository;
using Users_API.Services.Interface;

namespace Users_API.Controllers
{
    [Route("api/UserService/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRepository<User> _userRepository;
        private readonly IMapperService _mapperService;

        public UserController(IRepository<User> userRepository, IMapperService mapperService)
        {
            _userRepository = userRepository;
            _mapperService = mapperService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userRepository.GetAll();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserDTO userDTO)
        {
            var user = _mapperService.MapToEntity<UserDTO, User>(userDTO);
            await _userRepository.Add(user);
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserID }, user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO userDTO)
        {
            var existingUser = await _userRepository.GetById(id);
            if (existingUser == null) return NotFound();

            var user = _mapperService.MapToEntity<UserDTO, User>(userDTO);
            await _userRepository.Update(user);
            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound();
            await _userRepository.Delete(user);
            return NoContent();
        }
    }
}