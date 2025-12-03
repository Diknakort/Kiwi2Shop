using System.Collections.Generic;
// ProductsApi/Data/ProductsDbContext.cs
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using Kiwi2Shop.ProductsAPI.Models;
using System.Collections.Generic;


namespace Kiwi2Shop.ProductsAPI.Data
{


    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
    }

}
