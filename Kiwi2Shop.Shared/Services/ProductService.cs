using Kiwi2Shop.Shared.Dto;
using System.Net.Http.Json;

namespace Kiwi2Shop.ProductsAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task<List<ProductDto>> GetProductsByIdsAsync(List<Guid> productIds)
        {
            var response = await _httpClient.PostAsJsonAsync("api/products/batch", productIds);
            return await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new();
        }
    }
}
