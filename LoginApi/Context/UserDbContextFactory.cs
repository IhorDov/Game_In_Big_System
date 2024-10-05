using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace LoginApi.Context
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Adjust if necessary
                .AddJsonFile("appsettings.json")
                .Build();

            // Retrieve the connection string from the appsettings.json
            var connectionString = configuration.GetConnectionString("LoginDB");

            // Setup the DbContextOptionsBuilder to use Npgsql with the connection string
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Return the context with the options configured
            return new UserDbContext(optionsBuilder.Options);
        }
    }
}
