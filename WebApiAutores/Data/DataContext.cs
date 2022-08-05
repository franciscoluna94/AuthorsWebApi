using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entities;

namespace WebApiAutores.Data
{
    public class DataContext : IdentityDbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            base.OnModelCreating (modelBuilder);

            modelBuilder.Entity<AutorLibro>().HasKey(al => new { al.AutorId, al.LibroId });
        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; } // Opcional porque la tabla autores ya está relacionada con libros y se crearía automáticamente (pero no se podría hacer queries)
   
        public DbSet<Comentario> Comentarios { get; set; }

        public DbSet<AutorLibro> AutorLibro { get; set; }
    }
}
