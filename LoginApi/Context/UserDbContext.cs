using LoginApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginApi.Context
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
                

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configuration for the User entity can be done here
        }
    }
}
