using Kiwi2Shop.Orders.Data;
using Microsoft.AspNetCore.Mvc;

namespace Kiwi2Shop.Orders.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly Kiwi2ShopDbContext _context;

        public OrdersController(Kiwi2ShopDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Orders.Include(o => o.Items).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
        }
    }
}
