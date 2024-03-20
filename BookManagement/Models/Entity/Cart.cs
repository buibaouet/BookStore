namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Giỏ hàng
    /// </summary>
    public class Cart : BaseEntity
    {
        /// <summary>
        /// ID người mua
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// ID sách
        /// </summary>
        public int BookId { get; set; }
        /// <summary>
        /// Tên sách
        /// </summary>
        public string BookName { get; set; }
        /// <summary>
        /// Số lượng mua
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Giá mua 1 sản phẩm
        /// </summary>
        public int Price { get; set; }
        /// <summary>
        /// Ảnh bìa sách
        /// </summary>
        public string BookImage { get; set; }
    }
}
