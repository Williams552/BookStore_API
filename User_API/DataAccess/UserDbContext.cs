using Microsoft.EntityFrameworkCore;
using Users_API.Models;

namespace Users_API.DataAccess
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<WishList> WishLists { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ User - Cart
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserID);

            // Quan hệ User - WishList
            modelBuilder.Entity<WishList>()
                .HasOne(w => w.User)
                .WithMany(u => u.WishLists)
                .HasForeignKey(w => w.UserID);

            // Bỏ qua ràng buộc đến Books (thuộc BookService)
            modelBuilder.Entity<Cart>().Ignore(c => c.Book);
            modelBuilder.Entity<WishList>().Ignore(w => w.Book);
        }
    }
}