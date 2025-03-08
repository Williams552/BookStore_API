using Library_API.Models;
using Library_API.Repository;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IRepository<Book> _bookRepository;

        public BookController(IRepository<Book> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        // GET: api/<BookController>
        [HttpGet]
        public ActionResult<Book> GetAllBooks()
        {
            var books = _bookRepository.GetAll();
            if (books == null || !books.Any())
            {
                return NotFound("No books found.");
            }
            return Ok(books);
        }

        // GET api/<BookController>/5
        [HttpGet("{id}")]
        public ActionResult<Book> GetBookById(int id)
        {
            var author = _bookRepository.GetById(id);
            if (author == null)
            {
                return NotFound($"Author with ID {id} not found.");
            }
            return Ok(author);
        }

        // POST api/<BookController>
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            if (book == null)
            {
                return BadRequest("Author cannot be null.");
            }

            _bookRepository.Add(book);
            return CreatedAtAction(nameof(GetBookById), new { id = book.BookID }, book);
        }

        // PUT api/<BookController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<BookController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
