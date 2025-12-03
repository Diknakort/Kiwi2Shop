using System.Collections.Generic;

namespace Kiwi2Shop.Orders.Data
{
    public class Kiwi2ShopDbContext : DbContext, IKiwi2ShopDbContext
    {
        public Kiwi2ShopDbContext(DbContextOptions<Kiwi2ShopDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
    }
}
