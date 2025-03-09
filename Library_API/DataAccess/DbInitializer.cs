using BookStore_API.Models;

namespace BookStore_API.DataAccess
{
    public static class DbInitializer
    {
        public static void Initialize(BookStoreContext context)
        {
            // Kiểm tra và thêm dữ liệu mẫu vào bảng Authors nếu trống
            if (!context.Authors.Any())
            {
                var authors = new Author[]
                {
                    new Author { Fullname = "John Doe", Biography = "Biography of John Doe", ImageURL = "https://example.com/image1.jpg", CreateBy = "admin", CreateAt = DateTime.Now },
                    new Author { Fullname = "Jane Smith", Biography = "Biography of Jane Smith", ImageURL = "https://example.com/image2.jpg", CreateBy = "admin", CreateAt = DateTime.Now },
                    new Author { Fullname = "Michael Johnson", Biography = "Biography of Michael Johnson", ImageURL = "https://example.com/image3.jpg", CreateBy = "admin", CreateAt = DateTime.Now }
                };

                context.Authors.AddRange(authors);
                context.SaveChanges();
            }

            // Kiểm tra và thêm dữ liệu mẫu vào bảng Categories nếu trống
            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category { CategoryName = "Fiction", Description = "Books that tell fictional stories", CreateAt = DateTime.Now, IsDeleted = false },
                    new Category { CategoryName = "Non-fiction", Description = "Books based on factual information", CreateAt = DateTime.Now, IsDeleted = false },
                    new Category { CategoryName = "Science", Description = "Books related to scientific topics", CreateAt = DateTime.Now, IsDeleted = false }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Kiểm tra và thêm dữ liệu mẫu vào bảng Suppliers nếu trống
            if (!context.Suppliers.Any())
            {
                var suppliers = new Supplier[]
                {
                    new Supplier { SupplierName = "Book Supplier A", ContactName = "John Supplier", Phone = "123456789", Email = "supplierA@example.com", Address = "123 Supplier St.", CreateAt = DateTime.Now, IsDeleted = false },
                    new Supplier { SupplierName = "Book Supplier B", ContactName = "Jane Supplier", Phone = "987654321", Email = "supplierB@example.com", Address = "456 Supplier Ave.", CreateAt = DateTime.Now, IsDeleted = false }
                };

                context.Suppliers.AddRange(suppliers);
                context.SaveChanges();
            }

            // Kiểm tra và thêm dữ liệu mẫu vào bảng Books nếu trống
            if (!context.Books.Any())
            {
                var books = new Book[]
                {
                    new Book { Title = "Fictional Adventures", AuthorID = 1, CategoryID = 1, Price = 19.99m, Stock = 10, Description = "A thrilling fiction book.", ImageURL = "https://example.com/book1.jpg", SupplierID = 1, CreateAt = DateTime.Now, IsDeleted = false },
                    new Book { Title = "Science Discoveries", AuthorID = 2, CategoryID = 3, Price = 29.99m, Stock = 5, Description = "Explore the latest in science.", ImageURL = "https://example.com/book2.jpg", SupplierID = 2, CreateAt = DateTime.Now, IsDeleted = false }
                };

                context.Books.AddRange(books);
                context.SaveChanges();
            }

            // Kiểm tra và thêm dữ liệu mẫu vào bảng Users nếu trống
            if (!context.Users.Any())
            {
                var users = new User[]
                {
                    new User { Username = "user1", Password = "password1", Fullname = "Alice User", Role = "Customer", Email = "alice@example.com", Phone = "111222333", Address = "123 Main St.", CreateAt = DateTime.Now, IsDeleted = false },
                    new User { Username = "admin", Password = "admin123", Fullname = "Admin User", Role = "Admin", Email = "admin@example.com", Phone = "444555666", Address = "456 Admin Ave.", CreateAt = DateTime.Now, IsDeleted = false }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // Kiểm tra và thêm dữ liệu mẫu vào bảng Orders nếu trống
            if (!context.Orders.Any())
            {
                var orders = new Order[]
                {
                    new Order { UserID = 1, RecipientName = "Alice User", TotalAmount = 49.98m, DeliveryOption = "Standard", DeliveryAddress = "123 Main St.", RecipientPhone = "111222333", PaymentMethod = "Credit Card", Status = "Pending", CreateAt = DateTime.Now, IsDeleted = false }
                };

                context.Orders.AddRange(orders);
                context.SaveChanges();
            }

            // Kiểm tra và thêm dữ liệu mẫu vào bảng OrderDetails nếu trống
            if (!context.OrderDetails.Any())
            {
                var orderDetails = new OrderDetail[]
                {
                    new OrderDetail { OrderID = 1, BookID = 1, Quantity = 2 }
                };

                context.OrderDetails.AddRange(orderDetails);
                context.SaveChanges();
            }

            // Kiểm tra và thêm dữ liệu mẫu vào bảng Carts nếu trống
            if (!context.Carts.Any())
            {
                var carts = new Cart[]
                {
                    new Cart { UserID = 1, BookID = 1, Quantity = 1, CreateAt = DateTime.Now, IsDeleted = false }
                };

                context.Carts.AddRange(carts);
                context.SaveChanges();
            }
        }
    }
}
