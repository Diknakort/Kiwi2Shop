using Microsoft.EntityFrameworkCore;
using Kiwi2Shop.Shared;
using Kiwi2Shop.sAPI.Models;

namespace Kiwi2Shop.ProductsAPI.Services
{
    public class OrderService
    {
        private readonly OrdersDbContext _context;
        private readonly IProductService _productService;

        public OrderService(OrdersDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<Order> GetOrderWithProductsAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            // Obtener detalles de productos desde la otra API
            var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
            var products = await _productService.GetProductsByIdsAsync(productIds);

            // Enriquecer los OrderItems con datos de productos
            foreach (var item in order.OrderItems)
            {
                item.Product = products.FirstOrDefault(p => p.Id == item.ProductId);
            }

            return order;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in dto.Items)
            {
                // Validar que el producto existe
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    throw new Exception($"Product {item.ProductId} not found");

                order.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }
    }
}
