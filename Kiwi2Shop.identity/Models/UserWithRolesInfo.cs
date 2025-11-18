namespace Kiwi2Shop.identity.Models
{
    public class UserWithRolesInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
