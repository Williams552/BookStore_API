using BookStore_API.Models;

namespace BookStore_API.DataAccess
{
    public static class DbInitializer
    {
        public static void Initialize(BookStoreContext context)
        {
            // Thêm Authors
            if (!context.Authors.Any())
            {
                var authors = new Author[]
                {
            new Author { Fullname = "John Doe", Biography = "Tác giả best-seller", ImageURL = "/images/authors/1.jpg", CreateBy = "system", CreateAt = DateTime.Now },
            new Author { Fullname = "J.K. Rowling", Biography = "Tác giả Harry Potter", ImageURL = "/images/authors/2.jpg", CreateBy = "system", CreateAt = DateTime.Now },
            new Author { Fullname = "Stephen King", Biography = "Chuyên gia truyện kinh dị", ImageURL = "/images/authors/3.jpg", CreateBy = "system", CreateAt = DateTime.Now }
                };
                context.Authors.AddRange(authors);
                context.SaveChanges();
            }

            // Thêm Categories
            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
            new Category { CategoryName = "Tiểu thuyết", Description = "Tác phẩm hư cấu", CreateAt = DateTime.Now },
            new Category { CategoryName = "Khoa học", Description = "Sách khoa học", CreateAt = DateTime.Now },
            new Category { CategoryName = "Lập trình", Description = "Công nghệ thông tin", CreateAt = DateTime.Now }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Thêm Suppliers
            if (!context.Suppliers.Any())
            {
                var suppliers = new Supplier[]
                {
            new Supplier { SupplierName = "NXB Trẻ", ContactName = "Mr. A", Phone = "0901234567", Email = "nxbtre@example.com", Address = "TP.HCM" },
            new Supplier { SupplierName = "NXB Kim Đồng", ContactName = "Ms. B", Phone = "0917654321", Email = "kimdong@example.com", Address = "Hà Nội" }
                };
                context.Suppliers.AddRange(suppliers);
                context.SaveChanges();
            }

            // Thêm Books với quan hệ
            if (!context.Books.Any())
            {
                var books = new Book[]
                {
            new Book {
                Title = "Harry Potter",
                AuthorID = 2,
                CategoryID = 1,
                SupplierID = 1,
                Price = 150000,
                Stock = 100,
                Description = "Phiêu lưu phù thủy",
                ImageURL = "/images/books/1.jpg"
            },
            new Book {
                Title = "Clean Code",
                AuthorID = 1,
                CategoryID = 3,
                SupplierID = 2,
                Price = 200000,
                Stock = 50,
                Description = "Kỹ năng lập trình",
                ImageURL = "/images/books/2.jpg"
            }
                };
                context.Books.AddRange(books);
                context.SaveChanges();
            }

            // Thêm Users
            // Thêm Users với đầy đủ thông tin bắt buộc
            if (!context.Users.Any())
            {
                var users = new User[]
                {
            new User {
            Username = "admin",
            Password = "admin123",
            Fullname = "Quản trị hệ thống",
            Role = "Admin",
            Email = "admin@bookstore.com",
            Phone = "0900000001", // Thêm số điện thoại
            Address = "Trụ sở chính", // Thêm địa chỉ
            CreateAt = DateTime.Now,
            IsDeleted = false
        },
        new User {
            Username = "user1",
            Password = "user123",
            Fullname = "Người dùng 1",
            Role = "Customer",
            Email = "user1@bookstore.com",
            Phone = "0900000002", // Thêm số điện thoại
            Address = "123 Đường ABC", // Thêm địa chỉ
            CreateAt = DateTime.Now,
            IsDeleted = false
        }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // Thêm Orders và OrderDetails với quan hệ
            if (!context.Orders.Any())
            {
                var order = new Order
                {
                    UserID = 2,
                    RecipientName = "Trần Thị B",
                    TotalAmount = 500000,
                    DeliveryOption = "Giao hàng nhanh",
                    DeliveryAddress = "456 Đường XYZ, Quận 2",
                    RecipientPhone = "0918123456",
                    PaymentMethod = "Tiền mặt",
                    Status = "Chờ xác nhận",
                    CreateAt = DateTime.Now,
                    OrderDetails = new List<OrderDetail>
        {
            new OrderDetail {
                BookID = 1,
                Quantity = 2,
            },
            new OrderDetail {
                BookID = 2,
                Quantity = 1,
            }
        }
                };

                context.Orders.Add(order);
                context.SaveChanges();
            }

            // Thêm dữ liệu giỏ hàng
            if (!context.Carts.Any())
            {
                var cartItems = new Cart[]
                {
            new Cart { UserID = 2, BookID = 1, Quantity = 1 },
            new Cart { UserID = 2, BookID = 2, Quantity = 2 }
                };
                context.Carts.AddRange(cartItems);
                context.SaveChanges();
            }
        }
    }
}
