using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataAccess.Data
{
    public class BudgetDbContextFactory : IDesignTimeDbContextFactory<BudgetDBContext>
    {
        public BudgetDBContext CreateDbContext(string[] args)
        {
            // Zoek appsettings.json in het AppForLogin-project
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "AppForLogin");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<BudgetDBContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new BudgetDBContext(optionsBuilder.Options);
        }
    }
}



