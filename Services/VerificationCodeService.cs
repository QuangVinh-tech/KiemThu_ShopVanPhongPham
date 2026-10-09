using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Memory;

namespace ShopVanPhongPham.Services
{
    public class VerificationCodeService : IVerificationCodeService
    {
        public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
        public const int MaxAttempts = 5;

        private readonly IMemoryCache _cache;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<VerificationCodeService> _logger;

        public VerificationCodeService(
            IMemoryCache cache,
            IEmailSender emailSender,
            ILogger<VerificationCodeService> logger)
        {
            _cache = cache;
            _emailSender = emailSender;
            _logger = logger;
        }

        private sealed class CodeEntry
        {
            public byte[] Hash { get; init; } = Array.Empty<byte>();
            public DateTimeOffset SentAt { get; init; }
            public DateTimeOffset ExpiresAt { get; init; }
            public int Attempts { get; set; }
        }

        public async Task<SendCodeResult> SendAsync(string email, CodePurpose purpose)
        {
            var key = BuildKey(email, purpose);
            var now = DateTimeOffset.UtcNow;

            if (_cache.TryGetValue(key, out CodeEntry? old) && old != null)
            {
                var wait = (int)Math.Ceiling((old.SentAt + ResendCooldown - now).TotalSeconds);
                if (wait > 0)
                    return new SendCodeResult(SendCodeStatus.TooSoon, wait);
            }

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            _cache.Set(key, new CodeEntry
            {
                Hash = Hash(key, code),
                SentAt = now,
                ExpiresAt = now + CodeLifetime
            }, CodeLifetime);

            try
            {
                var (subject, body) = BuildEmail(purpose, code);
                await _emailSender.SendEmailAsync(email, subject, body);
                return new SendCodeResult(SendCodeStatus.Sent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không gửi được email mã xác nhận tới {Email}", email);
                _cache.Remove(key);
                return new SendCodeResult(SendCodeStatus.Failed);
            }
        }

        public VerifyCodeStatus Verify(string email, CodePurpose purpose, string? code)
        {
            var key = BuildKey(email, purpose);

            if (!_cache.TryGetValue(key, out CodeEntry? entry) || entry == null)
                return VerifyCodeStatus.Expired;

            lock (entry)
            {
                if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    _cache.Remove(key);
                    return VerifyCodeStatus.Expired;
                }

                if (entry.Attempts >= MaxAttempts)
                    return VerifyCodeStatus.TooManyAttempts;

                var input = Hash(key, (code ?? string.Empty).Trim());
                if (CryptographicOperations.FixedTimeEquals(entry.Hash, input))
                    return VerifyCodeStatus.Success;

                entry.Attempts++;
                return entry.Attempts >= MaxAttempts
                    ? VerifyCodeStatus.TooManyAttempts
                    : VerifyCodeStatus.Invalid;
            }
        }

        public void Remove(string email, CodePurpose purpose)
            => _cache.Remove(BuildKey(email, purpose));

        

        private static string BuildKey(string email, CodePurpose purpose)
            => $"otp:{purpose}:{(email ?? string.Empty).Trim().ToUpperInvariant()}";

        private static byte[] Hash(string key, string code)
            => SHA256.HashData(Encoding.UTF8.GetBytes($"{key}|{code}"));

        private static (string Subject, string Body) BuildEmail(CodePurpose purpose, string code)
        {
            var minutes = (int)CodeLifetime.TotalMinutes;

            var (subject, title, intro) = purpose switch
            {
                CodePurpose.VerifyEmail => (
                    "Mã xác minh tài khoản - VanPhamPro Shop",
                    "Xác minh email đăng ký",
                    "Cảm ơn bạn đã đăng ký tài khoản tại VanPhamPro Shop. Vui lòng nhập mã bên dưới để hoàn tất đăng ký:"),
                _ => (
                    "Mã đặt lại mật khẩu - VanPhamPro Shop",
                    "Đặt lại mật khẩu",
                    "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Mã xác nhận của bạn là:")
            };

            var body = $@"
<div style=""font-family:Arial,Helvetica,sans-serif;max-width:480px;margin:0 auto;padding:24px;border:1px solid #dee2e6;border-radius:8px;color:#212529"">
  <h2 style=""margin:0 0 12px;font-size:20px"">{title}</h2>
  <p style=""font-size:14px;line-height:1.6;margin:0 0 16px"">{intro}</p>
  <div style=""text-align:center;margin:20px 0"">
    <span style=""display:inline-block;font-size:34px;font-weight:700;letter-spacing:10px;padding:12px 24px;background:#f1f3f5;border-radius:8px"">{code}</span>
  </div>
  <p style=""font-size:13px;color:#6c757d;line-height:1.6;margin:0"">
    Mã có hiệu lực trong <b>{minutes} phút</b>. Tuyệt đối không chia sẻ mã này cho bất kỳ ai.<br/>
    Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.
  </p>
</div>";
            return (subject, body);
        }
    }
}