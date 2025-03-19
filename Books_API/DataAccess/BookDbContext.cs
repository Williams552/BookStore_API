using Books_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Books_API.DataAccess
{
    public class BookDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ Book - Author
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorID);
            // Quan hệ Book - Category
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryID);
            // Quan hệ Book - Publisher
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Supplier)
                .WithMany(s => s.Books)
                .HasForeignKey(b => b.SupplierID);
        }
    }
}