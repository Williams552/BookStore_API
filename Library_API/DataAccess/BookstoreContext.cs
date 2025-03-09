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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasOne(d => d.Author).WithMany(p => p.Books).HasForeignKey(d => d.AuthorID);

            entity.HasOne(d => d.Category).WithMany(p => p.Books).HasForeignKey(d => d.CategoryID);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Books).HasForeignKey(d => d.SupplierID);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasOne(d => d.Book).WithMany(p => p.Carts).HasForeignKey(d => d.BookID);

            entity.HasOne(d => d.User).WithMany(p => p.Carts).HasForeignKey(d => d.UserID);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.Orders).HasForeignKey(d => d.UserID);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasOne(d => d.Book).WithMany(p => p.OrderDetails).HasForeignKey(d => d.BookID);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails).HasForeignKey(d => d.OrderID);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
