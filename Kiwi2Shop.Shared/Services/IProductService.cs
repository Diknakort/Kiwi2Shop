using Kiwi2Shop.Shared.Dto;

namespace Kiwi2Shop.Shared.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
        Task<List<ProductDto>> GetProductsByIdsAsync(List<Guid> productIds);
    }
}
