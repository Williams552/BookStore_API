using Books_API.Domain.DTO;
using Books_API.Models;
using Books_API.Repository;
using Books_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Books_API.Controllers
{
    [Route("api/BookService/[controller]")]
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
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            IEnumerable<Book> books = await _bookRepository.GetAll(b => b.Author, b => b.Category, b => b.Supplier);
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
            var book = await _bookRepository.GetById(id);
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
                // Call the service to create the book
                var book = await _bookService.CreateBook(bookDTO);

                // Return the created book with a status of 201 (Created)
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
        // POST: api/book/list
        [HttpPost("list")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksByIds([FromBody] List<int> bookIds)
        {
            if (bookIds == null || !bookIds.Any())
            {
                return BadRequest("Book IDs cannot be null or empty.");
            }

            var books = await _bookRepository.GetByCondition(b => bookIds.Contains(b.BookID), b => b.Author, b => b.Category, b => b.Supplier);
            if (books == null || !books.Any())
            {
                return NotFound("No books found for the provided IDs.");
            }

            return Ok(books);
        }
        

    }
}
