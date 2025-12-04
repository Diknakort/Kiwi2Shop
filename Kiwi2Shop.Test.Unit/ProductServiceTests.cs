using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kiwi2Shop.ProductsAPI.Services;
using Kiwi2Shop.Shared.Dto;

namespace Kiwi2Shop.Test.Unit
{
    [TestClass]
    public sealed class ProductServiceTests
    {
        // Handler sencillo para simular respuestas HTTP
        private class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }

        [TestMethod]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenSuccess()
        {
            var id = Guid.NewGuid();
            var expected = new ProductDto { Id = id, Name = "TestProduct", Price = 9.99m };

            var handler = new FakeHandler(req =>
            {
                if (req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath.EndsWith($"/api/products/{id}"))
                {
                    var json = JsonSerializer.Serialize(expected);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new ProductService(client);

            var actual = await service.GetProductByIdAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual!.Id);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Price, actual.Price);
        }

        [TestMethod]
        public async Task GetProductsByIdsAsync_ReturnsList_WhenSuccess()
        {
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var expected = new List<ProductDto>
            {
                new ProductDto { Id = ids[0], Name = "A", Price = 1m },
                new ProductDto { Id = ids[1], Name = "B", Price = 2m }
            };

            var handler = new FakeHandler(req =>
            {
                if (req.Method == HttpMethod.Post && req.RequestUri!.AbsolutePath.EndsWith("/api/products/batch"))
                {
                    var json = JsonSerializer.Serialize(expected);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new ProductService(client);

            var actual = await service.GetProductsByIdsAsync(ids);

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count);
            CollectionAssert.AreEqual(expected.Select(p => p.Id).ToList(), actual.Select(p => p.Id).ToList());
        }
    }
}