﻿using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

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
            var user = await _userService.Register(userDTO);
            if (user == null)
            {
                return BadRequest("User already exists!");
            }
            return Ok(user);
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

        // GET: api/user
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

        // GET: api/user/{id}
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

        // POST: api/user
        //[HttpPost]
        //public async Task<ActionResult<User>> AddUser(UserDTO userDTO)
        //{
        //    var user = _mapperService.MapToDto<UserDTO, User>(userDTO);
        //    if (user == null)
        //    {
        //        return BadRequest("User cannot be null.");
        //    }

        //    await Task.Run(() => _userRepository.Add(user));
        //    return CreatedAtAction(nameof(GetUserById), new { id = user.UserID }, user);
        //}

        // PUT: api/user/{id}
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

            await Task.Run(() => _userRepository.Update(user));
            return NoContent();
        }

        // DELETE: api/user/{id}
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
    }
}
