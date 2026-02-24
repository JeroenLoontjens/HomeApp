using Microsoft.EntityFrameworkCore;
using WEB_USER_API.Models.Entities;

namespace WEB_USER_API.DATA
{
    public class API_DBContext : DbContext
    {

        public API_DBContext(DbContextOptions<API_DBContext> options) : base(options)
        {
        }

        public  DbSet<User> Users { get; set; }
        public DbSet<License> Licenses { get; set; }
    }
}
