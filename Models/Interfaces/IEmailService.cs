namespace ShopVanPhongPham.Models.Interfaces
{
    public interface IEmailService
    {
        Task<(bool success, string message)> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
