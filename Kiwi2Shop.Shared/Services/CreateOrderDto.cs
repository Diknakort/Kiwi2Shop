namespace Kiwi2Shop.Shared.Services
{
    public class CreateOrderDto
    {
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();

        public CreateOrderDto()
        {
        }

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