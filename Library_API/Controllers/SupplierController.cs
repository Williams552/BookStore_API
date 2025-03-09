using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IMapperService _mapperService;

        public SupplierController(IRepository<Supplier> supplierRepository, IMapperService mapperService)
        {
            _supplierRepository = supplierRepository;
            _mapperService = mapperService;
        }

        // GET: api/supplier
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetAllSuppliers()
        {
            var suppliers = await Task.Run(() => _supplierRepository.GetAll());
            if (suppliers == null || !suppliers.Any())
            {
                return NotFound("No suppliers found.");
            }
            return Ok(suppliers);
        }

        // GET: api/supplier/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplierById(int id)
        {
            var supplier = await Task.Run(() => _supplierRepository.GetById(id));
            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found.");
            }
            return Ok(supplier);
        }

        // POST: api/supplier
        [HttpPost]
        public async Task<ActionResult<Supplier>> AddSupplier(SupplierDTO supplierDTO)
        {
            var supplier = _mapperService.Map<SupplierDTO, Supplier>(supplierDTO);
            if (supplier == null)
            {
                return BadRequest("Supplier cannot be null.");
            }

            await Task.Run(() => _supplierRepository.Add(supplier));
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.SupplierID }, supplier);
        }

        // PUT: api/supplier/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, SupplierDTO supplierDTO)
        {
            if (id != supplierDTO.SupplierID)
            {
                return BadRequest("Supplier ID mismatch.");
            }

            var existingSupplier = await Task.Run(() => _supplierRepository.GetById(id));
            if (existingSupplier == null)
            {
                return NotFound($"Supplier with ID {id} not found.");
            }

            var supplier = _mapperService.Map<SupplierDTO, Supplier>(supplierDTO);

            await Task.Run(() => _supplierRepository.Update(supplier));
            return NoContent();
        }

        // DELETE: api/supplier/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await Task.Run(() => _supplierRepository.GetById(id));
            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found.");
            }

            await Task.Run(() => _supplierRepository.Delete(supplier));
            return NoContent();
        }
    }
}
