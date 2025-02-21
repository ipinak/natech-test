using Microsoft.EntityFrameworkCore;
using Natech.Caas.Database.Entities;

namespace Natech.Caas.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatEntity> Cats { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<CatTagEntity> CatTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatEntity>()
            .HasMany(c => c.Tags)
            .WithMany(c => c.Cats)
            .UsingEntity<CatTagEntity>();

        modelBuilder.Entity<TagEntity>()
            .HasIndex(t => t.Name)
            .IsUnique();
    }
}