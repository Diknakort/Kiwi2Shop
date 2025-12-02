using Kiwi2Shop.OrdersAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}

