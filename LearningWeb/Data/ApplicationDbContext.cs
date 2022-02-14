using LearningWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningWeb.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Category { get; set; }

    }
}
