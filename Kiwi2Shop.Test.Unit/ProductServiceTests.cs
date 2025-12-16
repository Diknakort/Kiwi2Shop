using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
//using Kiwi2Shop.ProductsAPI.Services;
using Kiwi2Shop.Shared.Services;
using Kiwi2Shop.Shared.Dto;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert; // Asegúrate de que esta directiva esté presente

namespace Kiwi2Shop.Test.Unit
{
    [TestFixture]
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

        [Test]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenSuccess()
        {
            var id = Guid.NewGuid();
            var expectedProduct = new ProductDto { Id = id, Name = "TestProduct", Price = 9.99m };

            var handler = new FakeHandler(req =>
            {
                if (req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath.EndsWith($"/api/products/{id}"))
                {
                    var json = JsonSerializer.Serialize(expectedProduct);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new ProductService(client);

            var actualProduct = await service.GetProductByIdAsync(id);

            Assert.That(actualProduct, Is.Not.Null);
            Assert.That(actualProduct!.Id, Is.EqualTo(expectedProduct.Id), "El Id no coincide.");
            Assert.That(actualProduct.Name, Is.EqualTo(expectedProduct.Name), "El Nombre no coincide.");
            // Utiliza una tolerancia para decimales si es necesario, aunque aquí Is.EqualTo es seguro.
            Assert.That(actualProduct.Price, Is.EqualTo(expectedProduct.Price), "El Precio no coincide.");
         }

        [Test]
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

            //Assert.IsNotNull(actual);
            //Assert.AreEqual(2, actual.Count);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Count, Is.EqualTo(expected.Count), "Count...");
            CollectionAssert.AreEqual(expected.Select(p => p.Id).ToList(), actual.Select(p => p.Id).ToList());
        }
    }
}