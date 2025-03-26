using BookStore_Client.Models;

namespace BookStore_Client.Models
{
    public class HomeViewModel
    {
        public List<Book> Books { get; set; }
        public List<Category> Categories { get; set; }
        public List<Author> Authors { get; set; } // Thêm danh sách tác giả
        public string Search { get; set; } // Từ khóa tìm kiếm
        public int? CategoryId { get; set; } // ID danh mục được chọn
        public int? AuthorId { get; set; } // ID tác giả được chọn
        public int CurrentPage { get; set; } // Trang hiện tại
        public int TotalPages { get; set; } // Tổng số trang
        public int PageSize { get; set; } // Số sách mỗi trang
    }
}