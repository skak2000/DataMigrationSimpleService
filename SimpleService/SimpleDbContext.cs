using Microsoft.EntityFrameworkCore;
using SimpleService.Configuration;
using SimpleService.Model;

namespace SimpleService
{
    public class SimpleDbContext : DbContext
    {
        public SimpleDbContext(DbContextOptions<SimpleDbContext> options)
        : base(options)
        {
        }

        public DbSet<SimpleAuthor> Authors { get; set; }
        public DbSet<SimpleBook> Books { get; set; }
        public DbSet<SimpleChapter> Chapters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SimpleAuthorConfiguration());
            modelBuilder.ApplyConfiguration(new SimpleBookConfiguration());
            modelBuilder.ApplyConfiguration(new SimpleChapterConfiguration());
        }
    }
}
