namespace ShopVanPhongPham.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        // Trạng thái: false = Chưa đọc, true = Đã đọc
        public bool IsRead { get; set; } = false;

        // Ngày gửi
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Ghi chú / phản hồi nội bộ của Admin (tuỳ chọn)
        public string? AdminNote { get; set; }
    }
}
