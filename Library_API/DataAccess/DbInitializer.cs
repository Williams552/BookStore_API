using Library_API.Models;

namespace Library_API.DataAccess
{
    public static class DbInitializer
    {
        public static void Initialize(BookstoreContext context)
        {
            // Kiểm tra xem cơ sở dữ liệu có tồn tại hay không
            if (context.Authors.Any())
            {
                return;   // Nếu đã có dữ liệu, không cần thêm nữa
            }

            // Thêm dữ liệu mẫu vào bảng Authors
            var authors = new Author[]
            {
                new Author { Fullname = "John Doe", Biography = "Biography of John Doe", ImageURL = "https://example.com/image1.jpg", CreateAt = DateTime.Now },
                new Author { Fullname = "Jane Smith", Biography = "Biography of Jane Smith", ImageURL = "https://example.com/image2.jpg", CreateAt = DateTime.Now },
                new Author { Fullname = "Michael Johnson", Biography = "Biography of Michael Johnson", ImageURL = "https://example.com/image3.jpg", CreateAt = DateTime.Now }
            };

            context.Authors.AddRange(authors);
            context.SaveChanges();
        }
    }
}
