namespace ShopVanPhongPham.Services
{
   
    public enum CodePurpose
    {
        VerifyEmail,    
        ResetPassword   
    }

    public enum SendCodeStatus { Sent, TooSoon, Failed }

    public enum VerifyCodeStatus { Success, Invalid, Expired, TooManyAttempts }

    public record SendCodeResult(SendCodeStatus Status, int WaitSeconds = 0);

    public interface IVerificationCodeService
    {
        
        Task<SendCodeResult> SendAsync(string email, CodePurpose purpose);

        
        VerifyCodeStatus Verify(string email, CodePurpose purpose, string? code);

        void Remove(string email, CodePurpose purpose);
    }
}