namespace Kiwi2Shop.identity.Data
{
    public class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";

        public static IEnumerable<string> GetAllRoles()
        {
            return new List<string> { Admin, User };
        }
    }
}
