using System;
using System.Collections.Generic;
using BookStore_API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore_API.DataAccess;

public partial class BookStoreContext : DbContext
{
    public BookStoreContext()
    {
    }

    public BookStoreContext(DbContextOptions<BookStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=TIENDAT;Initial Catalog=BookStore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            entity.Property(e => e.ImageURL).HasColumnName("ImageURL");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(e => e.AuthorID, "IX_Books_AuthorID");

            entity.HasIndex(e => e.CategoryID, "IX_Books_CategoryID");

            entity.HasIndex(e => e.SupplierID, "IX_Books_SupplierID");

            entity.Property(e => e.BookID).HasColumnName("BookID");
            entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            entity.Property(e => e.CategoryID).HasColumnName("CategoryID");
            entity.Property(e => e.ImageURL).HasColumnName("ImageURL");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SupplierID).HasColumnName("SupplierID");

            entity.HasOne(d => d.Author).WithMany(p => p.Books).HasForeignKey(d => d.AuthorID);

            entity.HasOne(d => d.Category).WithMany(p => p.Books).HasForeignKey(d => d.CategoryID);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Books).HasForeignKey(d => d.SupplierID);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(e => e.BookID, "IX_Carts_BookID");

            entity.HasIndex(e => e.UserID, "IX_Carts_UserID");

            entity.Property(e => e.CartID).HasColumnName("CartID");
            entity.Property(e => e.BookID).HasColumnName("BookID");
            entity.Property(e => e.UserID).HasColumnName("UserID");

            entity.HasOne(d => d.Book).WithMany(p => p.Carts).HasForeignKey(d => d.BookID);

            entity.HasOne(d => d.User).WithMany(p => p.Carts).HasForeignKey(d => d.UserID);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryID).HasColumnName("CategoryID");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.UserID, "IX_Orders_UserID");

            entity.Property(e => e.OrderID).HasColumnName("OrderID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserID).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders).HasForeignKey(d => d.UserID);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasIndex(e => e.BookID, "IX_OrderDetails_BookID");

            entity.HasIndex(e => e.OrderID, "IX_OrderDetails_OrderID");

            entity.Property(e => e.OrderDetailID).HasColumnName("OrderDetailID");
            entity.Property(e => e.BookID).HasColumnName("BookID");
            entity.Property(e => e.OrderID).HasColumnName("OrderID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Book).WithMany(p => p.OrderDetails).HasForeignKey(d => d.BookID);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails).HasForeignKey(d => d.OrderID);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(e => e.SupplierID).HasColumnName("SupplierID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserID).HasColumnName("UserID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
