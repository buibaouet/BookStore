﻿using static BookManagement.Constant.Enumerations;

namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Đơn hàng
    /// </summary>
    public class Order : BaseEntity
    {
        /// <summary>
        /// ID người mua
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Phí vận chuyển
        /// </summary>
        public int ShipCost { get; set; }
        /// <summary>
        /// Mã giảm giá (nếu có)
        /// </summary>
        public int? VoucherId { get; set; }
        /// <summary>
        /// Khuyến mại
        /// </summary>
        public int Discount { get; set; }
        /// <summary>
        /// Tổng tiền
        /// </summary>
        public int TotalMoney { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Lý do hủy đơn hàng
        /// </summary>
        public string CancelReason { get; set; }
    }
}