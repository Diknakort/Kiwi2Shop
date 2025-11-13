using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kiwi2Shop.identity.Data
{
    public class IdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=identitydb;Username=postgres;Password=postgres");
            return new ApplicationDbContext(optionsBuilder.Options);

        }
    }
}
