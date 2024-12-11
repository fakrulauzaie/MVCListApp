using ListApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListApp.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {   
        }

        public DbSet<Todo> Todos { get; set; }
    }
}
