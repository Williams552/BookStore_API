using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IRepository<Book> _bookRepository;
        private readonly IMapperService _mapperService;
        private readonly IBookService _bookService;

        public BookController(IRepository<Book> bookRepository, IMapperService mapperService, IBookService bookService)
        {
            _bookRepository = bookRepository;
            _mapperService = mapperService;
            _bookService = bookService;
        }

        // GET: api/book
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks(int? authorId)
        {
            var books = await _bookRepository.GetAll(b => b.Author, b => b.Category, b => b.Supplier);
            if (books == null || !books.Any())
            {
                return NotFound("No books found.");
            }

            if (authorId.HasValue)
            {
                books = books.Where(b => b.AuthorID == authorId.Value).ToList();
            }

            return Ok(books);
        }

        // GET: api/book/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _bookRepository.GetById(id, b => b.Author, b => b.Category, b => b.Supplier);
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
            if (bookDTO == null)
            {
                return BadRequest("Book cannot be null.");
            }

            try
            {
                var book = await _bookService.CreateBook(bookDTO);
                return CreatedAtAction(nameof(GetBookById), new { id = book.BookID }, book);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/book/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookDTO bookDTO)
        {
            if (id != bookDTO.BookID)
            {
                return BadRequest("Book ID mismatch.");
            }

            var existingBook = await _bookRepository.GetById(id);
            if (existingBook == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            var book = _mapperService.MapToEntity<BookDTO, Book>(bookDTO);
            await _bookRepository.Update(book);
            return NoContent();
        }

        // DELETE: api/book/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepository.GetById(id);
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            await _bookRepository.Delete(book);
            return NoContent();
        }
    }
}