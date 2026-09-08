using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ShopVanPhongPham.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public IdentityUser? User { get; set; }

        // Lưu đơn hàng nào chứng minh đã mua — bắt buộc để xác thực + chống review trùng
        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}