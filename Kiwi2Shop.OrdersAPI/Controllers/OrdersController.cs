using Asp.Versioning;
using Kiwi2Shop.sAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Kiwi2Shop.OrdersAPI.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext _context;

        public OrdersController(OrdersDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Orders.Include(o => o.OrderItems).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            _context.Orders.Add(entity: order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
        }
    }
}
