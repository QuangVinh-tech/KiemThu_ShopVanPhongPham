namespace ShopVanPhongPham.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public decimal OrderTotal { get; set; }
        public DateTime OrderPlaced { get; set; }
        public string? Status { get; set; }


        public List<OrderDetail> OrderDetails { get; set; } = new();
        public string PaymentMethod { get; set; } = "QR";      
        public string PaymentStatus { get; set; } = "Chưa thanh toán";
        public string? PromotionCode { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
    }
}
