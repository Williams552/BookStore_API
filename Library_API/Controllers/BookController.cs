using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IRepository<Book> _bookRepository;
        private readonly IMapperService _mapperService;

        public BookController(IRepository<Book> bookRepository, IMapperService mapperService)
        {
            _bookRepository = bookRepository;
            _mapperService = mapperService;
        }

        // GET: api/book
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var books = await Task.Run(() => _bookRepository.GetAll());
            if (books == null || !books.Any())
            {
                return NotFound("No books found.");
            }
            return Ok(books);
        }

        // GET: api/book/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await Task.Run(() => _bookRepository.GetById(id));
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }
            return Ok(book);
        }

        // POST: api/book
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(BookDTO bookDTO)
        {
            var book = _mapperService.Map<BookDTO, Book>(bookDTO);
            if (book == null)
            {
                return BadRequest("Book cannot be null.");
            }

            await Task.Run(() => _bookRepository.Add(book));
            return CreatedAtAction(nameof(GetBookById), new { id = book.BookID }, book);
        }

        // PUT: api/book/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookDTO bookDTO)
        {
            if (id != bookDTO.BookID)
            {
                return BadRequest("Book ID mismatch.");
            }

            var existingBook = await Task.Run(() => _bookRepository.GetById(id));
            if (existingBook == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            var book = _mapperService.Map<BookDTO, Book>(bookDTO);

            await Task.Run(() => _bookRepository.Update(book));
            return NoContent();
        }

        // DELETE: api/book/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await Task.Run(() => _bookRepository.GetById(id));
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            await Task.Run(() => _bookRepository.Delete(book));
            return NoContent();
        }
    }
}
