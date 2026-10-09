using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Models.Services
{
    public class OtpService : IOtpService
    {
        private const int ExpiryMinutes = 5;
        private const int ResendCooldownSeconds = 60;
        private const int MaxAttempts = 5;

        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OtpService> _logger;

        private class OtpEntry
        {
            public string Hash { get; set; } = "";
            public DateTime ExpiresAt { get; set; }
            public DateTime NextResendAt { get; set; }
            public int Attempts { get; set; }
        }

        public OtpService(IMemoryCache cache, IEmailService emailService,
                          IWebHostEnvironment env, ILogger<OtpService> logger)
        {
            _cache = cache;
            _emailService = emailService;
            _env = env;
            _logger = logger;
        }

        private static string Key(string purpose, string email)
            => $"otp:{purpose}:{email.Trim().ToLowerInvariant()}";

        private static string Hash(string key, string code)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{key}:{code}"));
            return Convert.ToHexString(bytes);
        }

        public async Task<(bool success, string message)> SendOtpAsync(string email, string purpose)
        {
            var key = Key(purpose, email);
            var now = DateTime.UtcNow;

            if (_cache.TryGetValue(key, out OtpEntry? existing) && existing != null
                && existing.NextResendAt > now)
            {
                var wait = (int)Math.Ceiling((existing.NextResendAt - now).TotalSeconds);
                return (false, $"Vui lòng đợi {wait} giây rồi gửi lại mã.");
            }

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            var entry = new OtpEntry
            {
                Hash = Hash(key, code),
                ExpiresAt = now.AddMinutes(ExpiryMinutes),
                NextResendAt = now.AddSeconds(ResendCooldownSeconds),
                Attempts = 0
            };
            _cache.Set(key, entry, TimeSpan.FromMinutes(ExpiryMinutes));

            var (subject, action) = purpose == OtpPurposes.ResetPassword
                ? ("Mã OTP đặt lại mật khẩu - VanPhamPro Shop", "đặt lại mật khẩu")
                : ("Mã OTP xác thực tài khoản - VanPhamPro Shop", "xác thực tài khoản");

            var body = $@"
<div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #e5e5e5;border-radius:8px'>
  <h2 style='margin-top:0'>VanPhamPro Shop</h2>
  <p>Mã OTP để {action} của bạn là:</p>
  <p style='font-size:32px;font-weight:bold;letter-spacing:8px;text-align:center;margin:16px 0'>{code}</p>
  <p>Mã có hiệu lực trong {ExpiryMinutes} phút. Tuyệt đối không chia sẻ mã này cho bất kỳ ai.</p>
  <p style='color:#888;font-size:12px'>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
</div>";

            var (sent, message) = await _emailService.SendEmailAsync(email, subject, body);
            if (!sent)
            {
                if (_env.IsDevelopment())
                {
                    // Chỉ khi chạy trên máy dev: in mã ra Output để test khi chưa cấu hình được SMTP
                    _logger.LogWarning("[DEV] OTP ({Purpose}) cho {Email}: {Code}", purpose, email, code);
                    return (true, "Mã OTP đã được tạo (môi trường dev: xem mã trong cửa sổ Output).");
                }

                _cache.Remove(key);
                return (false, message);
            }

            return (true, "Mã OTP đã được gửi. Vui lòng kiểm tra hộp thư (kể cả mục spam).");
        }

        public OtpVerifyResult Verify(string email, string purpose, string code, bool consumeOnSuccess = true)
        {
            var key = Key(purpose, email);

            if (!_cache.TryGetValue(key, out OtpEntry? entry) || entry == null)
                return OtpVerifyResult.Expired;

            lock (entry)
            {
                if (entry.ExpiresAt < DateTime.UtcNow)
                {
                    _cache.Remove(key);
                    return OtpVerifyResult.Expired;
                }

                if (entry.Attempts >= MaxAttempts)
                    return OtpVerifyResult.TooManyAttempts;

                var expected = Encoding.UTF8.GetBytes(entry.Hash);
                var actual = Encoding.UTF8.GetBytes(Hash(key, (code ?? "").Trim()));

                if (!CryptographicOperations.FixedTimeEquals(expected, actual))
                {
                    entry.Attempts++;
                    return entry.Attempts >= MaxAttempts
                        ? OtpVerifyResult.TooManyAttempts
                        : OtpVerifyResult.Invalid;
                }

                if (consumeOnSuccess)
                    _cache.Remove(key);

                return OtpVerifyResult.Success;
            }
        }

        public void Invalidate(string email, string purpose)
            => _cache.Remove(Key(purpose, email));
    }
}