using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using FinalWebapp.Data;

namespace FinalWebapp.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=fitness.db");
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}