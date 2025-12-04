namespace Kiwi2Shop.ProductsAPI.Services
{
    public class CreateOrderDto
    {
        public List<CreateOrderItemDto> Items { get; set; }

        public CreateOrderDto(List<CreateOrderItemDto> items)
        {
            Items = items;
        }
    }

    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}