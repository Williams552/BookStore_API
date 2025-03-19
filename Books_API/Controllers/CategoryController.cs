using BookStore_API.Domain.DTO;
using Books_API.Models;
using Books_API.Repository;
using Books_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Books_API.Domain.DTO;

namespace Books_API.Books_API
{
    [Route("api/BookService/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapperService _mapperService;

        public CategoryController(IRepository<Category> categoryRepository, IMapperService mapperService)
        {
            _categoryRepository = categoryRepository;
            _mapperService = mapperService;
        }

        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAll(c => c.Books);
            if (categories == null || !categories.Any())
            {
                return NotFound("No categories found.");
            }
            return Ok(categories);
        }

        // GET: api/category/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await Task.Run(() => _categoryRepository.GetById(id, c => c.Books));
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return Ok(category);
        }

        // POST: api/category
        [HttpPost]
        public async Task<ActionResult<Category>> AddCategory(CategoryDTO categoryDTO)
        {
            var category = _mapperService.MapToDto<CategoryDTO, Category>(categoryDTO);
            if (category == null)
            {
                return BadRequest("Category cannot be null.");
            }

            await Task.Run(() => _categoryRepository.Add(category));
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryID }, category);
        }

        // PUT: api/category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDTO categoryDTO)
        {
            if (id != categoryDTO.CategoryID)
            {
                return BadRequest("Category ID mismatch.");
            }

            var existingCategory = await Task.Run(() => _categoryRepository.GetById(id));
            if (existingCategory == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            var category = _mapperService.MapToDto<CategoryDTO, Category>(categoryDTO);

            await Task.Run(() => _categoryRepository.Update(category));
            return NoContent();
        }

        // DELETE: api/category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await Task.Run(() => _categoryRepository.GetById(id));
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            await Task.Run(() => _categoryRepository.Delete(category));
            return NoContent();
        }
    }
}
