using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.InMemory;
using Kiwi2Shop.ProductsAPI.Services;
using Kiwi2Shop.Shared.Dto;

// OrdersDbContext está definido en el proyecto como tipo global (sin namespace)
#pragma warning disable CS8632
#pragma warning restore CS8632

namespace Kiwi2Shop.Test.Unit
{
    [TestClass]
    public sealed class OrderServiceTests
    {
        private class FakeProductService : IProductService
        {
            private readonly Dictionary<Guid, ProductDto> _products;

            public FakeProductService(IEnumerable<ProductDto> products)
            {
                _products = products.ToDictionary(p => p.Id);
            }

            public Task<ProductDto?> GetProductByIdAsync(Guid productId)
            {
                _products.TryGetValue(productId, out var p);
                return Task.FromResult<ProductDto?>(p);
            }

            public Task<List<ProductDto>> GetProductsByIdsAsync(List<Guid> productIds)
            {
                var list = productIds.Where(id => _products.ContainsKey(id))
                                     .Select(id => _products[id])
                                     .ToList();
                return Task.FromResult(list);
            }
        }

        //[TestMethod]
        //public async Task CreateOrderAsync_SavesOrderWithItemsAndUnitPrice()
        //{
        //    // Arrange
        //    var productId = Guid.NewGuid();
        //    var product = new ProductDto { Id = productId, Name = "Test", Price = 12.5m };
        //    var productService = new FakeProductService(new[] { product });

        //    var options = new DbContextOptionsBuilder<OrdersDbContext>()
        //        .UseInMemoryDatabase(databaseName: $"Orders_Create_{Guid.NewGuid()}")
        //        .Options;

        //    await using var context = new OrdersDbContext(options);

        //    var service = new OrderService(context, productService);

        //    var dto = new CreateOrderDto(new List<CreateOrderItemDto>
        //    {
        //        new CreateOrderItemDto { ProductId = productId, Quantity = 3 }
        //    });

        //    // Act
        //    var created = await service.CreateOrderAsync(dto);

        //    // Assert
        //    Assert.IsNotNull(created);
        //    Assert.AreNotEqual(Guid.Empty, created.Id);
        //    Assert.AreEqual(1, created.OrderItems.Count);

        //    var item = created.OrderItems[0];
        //    Assert.AreEqual(productId, item.ProductId);
        //    Assert.AreEqual(3, item.Quantity);
        //    Assert.AreEqual(product.Price, item.UnitPrice);

        //    // Verify persisted in context
        //    var persisted = await context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == created.Id);
        //    Assert.IsNotNull(persisted);
        //    Assert.AreEqual(1, persisted!.OrderItems.Count);
        //    Assert.AreEqual(product.Price, persisted.OrderItems[0].UnitPrice);
        //}

        //[TestMethod]
        //public async Task GetOrderWithProductsAsync_PopulatesOrderItemsProduct()
        //{
        //    // Arrange
        //    var productId = Guid.NewGuid();
        //    var product = new ProductDto { Id = productId, Name = "ProductX", Price = 7.5m };
        //    var productService = new FakeProductService(new[] { product });

        //    var options = new DbContextOptionsBuilder<OrdersDbContext>()
        //        .UseInMemoryDatabase(databaseName: $"Orders_Get_{Guid.NewGuid()}")
        //        .Options;

        //    await using (var context = new OrdersDbContext(options))
        //    {
        //        var order = new Kiwi2Shop.sAPI.Models.Order
        //        {
        //            Id = Guid.NewGuid(),
        //            CreatedAt = DateTime.UtcNow,
        //            OrderItems = new List<Kiwi2Shop.sAPI.Models.OrderItem>
        //            {
        //                new Kiwi2Shop.sAPI.Models.OrderItem
        //                {
        //                    Id = Guid.NewGuid(),
        //                    OrderId = Guid.NewGuid(), // will be overwritten by container
        //                    ProductId = productId,
        //                    Quantity = 2,
        //                    UnitPrice = product.Price
        //                }
        //            }
        //        };

        //        // Ensure OrderItem.OrderId matches Order.Id
        //        order.OrderItems[0].OrderId = order.Id;

        //        context.Orders.Add(order);
        //        await context.SaveChangesAsync();
        //    }

        //    await using (var context = new OrdersDbContext(options))
        //    {
        //        var service = new OrderService(context, productService);

        //        // Act
        //        var orderFromService = await service.GetOrderWithProductsAsync(
        //            (await context.Orders.Select(o => o.Id).FirstAsync()));

        //        // Assert
        //        Assert.IsNotNull(orderFromService);
        //        Assert.IsTrue(orderFromService!.OrderItems.Count > 0);
        //        var oi = orderFromService.OrderItems[0];
        //        Assert.IsNotNull(oi.Product);
        //        Assert.AreEqual(productId, oi.Product!.Id);
        //        Assert.AreEqual(product.Name, oi.Product.Name);
        //        Assert.AreEqual(product.Price, oi.Product.Price);
        //    }
        //}
    }
}