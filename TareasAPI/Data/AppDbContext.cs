using Microsoft.EntityFrameworkCore;
using TareasAPI.Models;

namespace TareasAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tarea>().HasKey(t => t.Id);
        modelBuilder.Entity<Tarea>().Property(t => t.Titulo).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Tarea>().Property(t => t.Descripcion).HasMaxLength(500);
    }
}
