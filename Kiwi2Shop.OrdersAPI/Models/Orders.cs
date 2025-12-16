using Kiwi2Shop.Shared;
using Kiwi2Shop.Shared.Dto;



namespace Kiwi2Shop.sAPI.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }  // Referencia externa
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Propiedades calculadas desde la API de Products
        public ProductDto? Product { get; set; }
    }



}
