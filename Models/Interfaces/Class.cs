namespace ShopVanPhongPham.Models.Interfaces
{
    public enum OtpVerifyResult
    {
        Success,
        Invalid,
        Expired,
        TooManyAttempts
    }

    public static class OtpPurposes
    {
        public const string Register = "register";
        public const string ResetPassword = "reset";
    }

    public static class OtpMessages
    {
        public static string For(OtpVerifyResult result) => result switch
        {
            OtpVerifyResult.Invalid => "Mã OTP không đúng. Vui lòng kiểm tra lại.",
            OtpVerifyResult.Expired => "Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng gửi lại mã mới.",
            OtpVerifyResult.TooManyAttempts => "Bạn đã nhập sai quá nhiều lần. Vui lòng đợi rồi gửi lại mã mới.",
            _ => ""
        };
    }

    public interface IOtpService
    {
        Task<(bool success, string message)> SendOtpAsync(string email, string purpose);
        OtpVerifyResult Verify(string email, string purpose, string code, bool consumeOnSuccess = true);
        void Invalidate(string email, string purpose);
    }
}