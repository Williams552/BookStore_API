using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IRepository<Author> _authorRepository;
        private readonly IMapperService _mapperService;

        public AuthorController(IRepository<Author> authorRepository, IMapperService mapperService)
        {
            _authorRepository = authorRepository;
            _mapperService = mapperService;
        }

        // GET: api/author
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAllAuthors()
        {
            var authors = await Task.Run(() => _authorRepository.GetAll());
            if (authors == null || !authors.Any())
            {
                return NotFound("No authors found.");
            }
            return Ok(authors);
        }

        // GET: api/author/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Author>> GetAuthorById(int id)
        {
            var author = await Task.Run(() => _authorRepository.GetById(id));
            if (author == null)
            {
                return NotFound($"Author with ID {id} not found.");
            }
            return Ok(author);
        }

        // POST: api/author
        [HttpPost]
        public async Task<ActionResult<Author>> AddAuthor(AuthorDTO authorDTO)
        {
            var author = _mapperService.Map<AuthorDTO, Author>(authorDTO);
            if (author == null)
            {
                return BadRequest("Author cannot be null.");
            }

            await Task.Run(() => _authorRepository.Add(author));
            return CreatedAtAction(nameof(GetAuthorById), new { id = author.AuthorID }, author);
        }

        // PUT: api/author/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorDTO authorDTO)
        {
            if (id != authorDTO.AuthorID)
            {
                return BadRequest("Author ID mismatch.");
            }

            var existingAuthor = await Task.Run(() => _authorRepository.GetById(id));
            if (existingAuthor == null)
            {
                return NotFound($"Author with ID {id} not found.");
            }

            var author = _mapperService.Map<AuthorDTO, Author>(authorDTO);

            await Task.Run(() => _authorRepository.Update(author));
            return NoContent();
        }

        // DELETE: api/author/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await Task.Run(() => _authorRepository.GetById(id));
            if (author == null)
            {
                return NotFound($"Author with ID {id} not found.");
            }

            await Task.Run(() => _authorRepository.Delete(author));
            return NoContent();
        }
    }
}
