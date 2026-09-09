using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Models.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<(bool success, string message)> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = _settings.EnableSsl
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                return (true, "Đã gửi email thành công.");
            }
            catch (Exception ex)
            {
                // Log lỗi thật để dev xem trong Output/Console, nhưng KHÔNG lộ chi tiết SMTP ra ngoài cho người dùng
                _logger.LogError(ex, "Gửi email tới {ToEmail} thất bại", toEmail);
                return (false, "Không thể gửi email lúc này. Vui lòng kiểm tra lại cấu hình SMTP (EmailSettings trong appsettings.json).");
            }
        }
    }
}
