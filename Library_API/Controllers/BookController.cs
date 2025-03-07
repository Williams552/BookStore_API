using Library_API.Models;
using Library_API.Repository;
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

        public AuthorController(IRepository<Author> authorRepository)
        {
            _authorRepository = authorRepository;
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
        public async Task<ActionResult<Author>> AddAuthor(Author author)
        {
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
