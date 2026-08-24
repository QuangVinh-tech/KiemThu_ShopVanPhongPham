namespace ShopVanPhongPham.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int DiscountPercent { get; set; }
        public string ImageUrl { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public string Code { get; set; } = "";
    }
}