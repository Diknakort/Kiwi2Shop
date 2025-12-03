using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Kiwi2Shop.OrdersAPI.Models;


namespace Kiwi2Shop.OrdersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext _context;

        public OrdersController(OrdersDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Orders.Include(o => o.Items).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            _context.Orders.Add(entity: order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
        }
    }
}
