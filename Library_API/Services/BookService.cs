using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Google.Apis.Storage.v1.Data;

namespace BookStore_API.Services
{
    public class BookService : IBookService
    {
        private readonly IRepository<Book> _bookRepository;
        private readonly IMapperService _mapperService;
        private readonly IRepository<Author> _authorRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<Category> _categoryRepository;
        public BookService(IRepository<Book> bookRepository,
            IMapperService mapperService,
            IRepository<Author> authorRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<Category> categoryRepository)
        {
            _bookRepository = bookRepository;
            _mapperService = mapperService;
            _authorRepository = authorRepository;
            _supplierRepository = supplierRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<BookDTO> CreateBook(BookDTO bookDTO)
        {
            var book = _mapperService.MapToDto<BookDTO, BookDTO>(bookDTO);

            if (book == null)
            {
                throw new ArgumentException("Book cannot be null.");
            }

            var author = await _authorRepository.GetById(bookDTO.AuthorID);

            if (author == null)
            {
                throw new ArgumentException($"Author with ID {bookDTO.AuthorID} not found.");
            }

            var supplier = await _supplierRepository.GetById(bookDTO.SupplierID);

            if (supplier == null)
            {
                throw new ArgumentException($"Supplier with ID {bookDTO.SupplierID} not found.");
            }

            var category = await _categoryRepository.GetById(bookDTO.CategoryID);

            if (category == null)
            {
                throw new ArgumentException($"Category with ID {bookDTO.CategoryID} not found.");
            }

            book.Author = author;
            book.Supplier = supplier;
            book.Category = category;
            await Task.Run(() => _bookRepository.Add(book));
            return book;
        }
    }
}
