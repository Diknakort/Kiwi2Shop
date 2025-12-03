namespace Kiwi2Shop.Orders.Data
{
    public interface IKiwi2ShopDbContext
    {
        DbSet<Order> Orders { get; }
        DbSet<Product> Products { get; }
    }
}