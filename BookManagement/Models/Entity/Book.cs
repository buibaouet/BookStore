namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Sách
    /// </summary>
    public class Book : BaseEntity
    {
        /// <summary>
        /// Mã sách
        /// </summary>
        public string BookCode { get; set; }
        /// <summary>
        /// Tên sách
        /// </summary>
        public string BookName { get; set; }
        /// <summary>
        /// Mã danh mục
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// Tác giả
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// Nhà xuất bản
        /// </summary>
        public string Publisher { get; set; }
        /// <summary>
        /// Năm xuất bản
        /// </summary>
        public string PublishTime { get; set; }
        /// <summary>
        /// Trọng lượng
        /// </summary>
        public double? BookWeight { get; set; }
        /// <summary>
        /// Kích thước
        /// </summary>
        public string? BookSize { get; set; }
        /// <summary>
        /// Số trang
        /// </summary>
        public double? BookPage { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Còn lại
        /// </summary>
        public int Remaining { get; set; }
        /// <summary>
        /// Giá bán
        /// </summary>
        public int Price { get; set; }
        /// <summary>
        /// Giá Khuyến mại
        /// </summary>
        public int? PriceDiscount { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Ảnh bìa sách
        /// </summary>
        public string BookImage { get; set; }
        /// <summary>
        /// Các ảnh thêm
        /// </summary>
        public string InfoImage { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; }
    }
}
