namespace Kiwi2Shop.GateWay
{
    public class GatewayConfiguration
    {
        public string IdentityApiUrl { get; set; } = "http://localhost:5001";
        // Agregar más servicios aquí en el futuro
        public string ProductsApiUrl { get; set; } = "http://localhost:5002";
        public string OrdersApiUrl { get; set; } = "http://localhost:5003";
    }
}
