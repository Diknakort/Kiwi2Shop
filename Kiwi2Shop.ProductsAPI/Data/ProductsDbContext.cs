// ProductsApi/Data/ProductsDbContext.cs
using Kiwi2Shop.ProductsAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace Kiwi2Shop.ProductsAPI.Data
{


    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
    }

}
