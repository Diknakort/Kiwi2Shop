namespace GranadaShopClase.API.Identity.Dto.Auth
{
    public class ResponseLogin
    {
        public required string Token { get; set; }
        public required DateTime Expiration { get; set; }

    }
}
