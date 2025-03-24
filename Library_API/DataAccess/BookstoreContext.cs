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

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WishList> WishLists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            entity.Property(e => e.Biography).HasColumnType("text");
            entity.Property(e => e.ImageURL).HasColumnName("ImageURL");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(e => e.BookID).HasColumnName("BookID");
            entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            entity.Property(e => e.CategoryID).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.ImageURL).HasColumnName("ImageURL");
            entity.Property(e => e.IsDelete).HasColumnName("isDelete");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SupplierID).HasColumnName("SupplierID");

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorID)
                .HasConstraintName("FK_Books_Authors");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryID)
                .HasConstraintName("FK_Books_Categories");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Books)
                .HasForeignKey(d => d.SupplierID)
                .HasConstraintName("FK_Books_Suppliers");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Cart");

            entity.Property(e => e.CartID).HasColumnName("CartID");
            entity.Property(e => e.BookID).HasColumnName("BookID");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserID).HasColumnName("UserID");

            entity.HasOne(d => d.Book).WithMany(p => p.Carts)
                .HasForeignKey(d => d.BookID)
                .HasConstraintName("FK_Cart_Books");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserID)
                .HasConstraintName("FK_Cart_User");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryID).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasColumnType("text");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(e => e.SupplierID).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.SupplierName).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.UserID).HasColumnName("UserID");
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
            entity.Property(e => e.IsDelete).HasColumnName("isDelete");
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<WishList>(entity =>
        {
            entity.HasKey(e => e.WishID);

            entity.ToTable("WishList");

            entity.Property(e => e.WishID).HasColumnName("WishID");
            entity.Property(e => e.BookID).HasColumnName("BookID");
            entity.Property(e => e.UserID).HasColumnName("UserID");

            entity.HasOne(d => d.Book).WithMany(p => p.WishLists)
                .HasForeignKey(d => d.BookID)
                .HasConstraintName("FK_WishList_Books");

            entity.HasOne(d => d.User).WithMany(p => p.WishLists)
                .HasForeignKey(d => d.UserID)
                .HasConstraintName("FK_WishList_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
