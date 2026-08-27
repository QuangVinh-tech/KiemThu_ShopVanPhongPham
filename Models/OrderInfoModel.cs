namespace ShopVanPhongPham.Models
{
    public class OrderInfoModel
    {
        public string OrderId { get; set; } = "";
        public string FullName { get; set; } = "";
        public double Amount { get; set; }
        public string OrderInfo { get; set; } = "";
    }

    public class MomoCreatePaymentResponseModel
    {
        public string PartnerCode { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string RequestId { get; set; } = "";
        public int Amount { get; set; }
        public long ResponseTime { get; set; }
        public string Message { get; set; } = "";
        public int ResultCode { get; set; }
        public string PayUrl { get; set; } = "";
    }

    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public double Amount { get; set; }
        public string OrderInfo { get; set; } = "";
    }
}