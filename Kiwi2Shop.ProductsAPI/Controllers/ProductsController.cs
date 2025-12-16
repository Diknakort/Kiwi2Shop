using Asp.Versioning;
using Kiwi2Shop.ProductsAPI.Data;
using Kiwi2Shop.ProductsAPI.Models;
using Kiwi2Shop.Shared.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiwi2Shop.ProductsAPI.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class ProductsController : ControllerBase
    {
        private readonly ProductsDbContext _context;

        public ProductsController(ProductsDbContext context) => _context = context;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Products.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPost("batch")]
        public async Task<IActionResult> GetBatch([FromBody] List<Guid> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return BadRequest("Product IDs list is empty");

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var dtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price
            }).ToList();

            return Ok(dtos);
        }
    }
}
