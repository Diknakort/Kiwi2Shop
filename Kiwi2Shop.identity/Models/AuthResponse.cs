namespace Kiwi2Shop.Identity.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
    }
}


