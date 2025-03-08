using Library_API.Domain.DTO;
using Library_API.Models;
using Library_API.Repository;
using Library_API.Services;
using Library_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library_API.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IRepository<Author> _authorRepository;
        private readonly IAuthorServices _authorServices;

        public AuthorController(IRepository<Author> authorRepository, IAuthorServices authorServices)
        {
            _authorRepository = authorRepository;
            _authorServices = authorServices;
        }

        // GET: api/author
        [HttpGet]
        public ActionResult<IEnumerable<Author>> GetAllAuthors()
        {
            var authors = _authorRepository.GetAll();
            if (authors == null || !authors.Any())
            {
                return NotFound("No authors found.");
            }
            return Ok(authors);
        }

        // GET: api/author/{id}
        [HttpGet("{id}")]
        public ActionResult<Author> GetAuthorById(int id)
        {
            var author = _authorRepository.GetById(id);
            if (author == null)
            {
                return NotFound($"Author with ID {id} not found.");
            }
            return Ok(author);
        }

        // POST: api/author
        [HttpPost]
        public async Task<ActionResult<Author>> AddAuthor(AuthorCreateDTO authorDTO)
        {
            var author = _authorServices.AuthorDTOtoAuthor(authorDTO);
            if (author == null)
            {
                return BadRequest("Author cannot be null.");
            }

            _authorRepository.Add(author);
            return CreatedAtAction(nameof(GetAuthorById), new { id = author.AuthorID }, author);
        }

        // PUT: api/author/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAuthor(int id, Author author)
        {
            if (id != author.AuthorID)
            {
                return BadRequest("Author ID mismatch.");
            }

            var existingAuthor = _authorRepository.GetById(id);
            if (existingAuthor == null)
            {
                return NotFound($"Author with ID {id} not found.");
            }

            _authorRepository.Update(author);
            return NoContent();
        }

        // DELETE: api/author/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(int id)
        {
            var author = _authorRepository.GetById(id);
            if (author == null)
            {
                return NotFound($"Author with ID {id} not found.");
            }

            _authorRepository.Delete(author);
            return NoContent();
        }
    }

}
