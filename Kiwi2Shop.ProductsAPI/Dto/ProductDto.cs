namespace Kiwi2Shop.ProductsAPI.Dto
{
    // DTO para datos del producto
    public class ProductDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}