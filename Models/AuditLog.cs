namespace ShopVanPhongPham.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // VD: "Xác nhận thanh toán VietQR", "Cập nhật trạng thái đơn hàng"...
        public string Action { get; set; } = "";

        // Email/username của admin thực hiện thao tác
        public string PerformedBy { get; set; } = "";

        public DateTime PerformedAt { get; set; } = DateTime.Now;

        // Ghi chú thêm, VD: "Trạng thái thanh toán: Chưa thanh toán -> Đã thanh toán"
        public string? Note { get; set; }
    }
}
