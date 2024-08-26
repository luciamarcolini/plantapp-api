using Microsoft.EntityFrameworkCore;
using PlantAPI.Models;

namespace PlantAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Capture> Capture { get; set; }
    }
}
