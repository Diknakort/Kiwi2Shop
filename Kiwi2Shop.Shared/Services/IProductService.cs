using Kiwi2Shop.Shared.Dto;

namespace Kiwi2Shop.ProductsAPI.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
        Task<List<ProductDto>> GetProductsByIdsAsync(List<Guid> productIds);
    }
}
