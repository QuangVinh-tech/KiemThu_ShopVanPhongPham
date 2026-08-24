namespace ShopVanPhongPham.Helpers
{
    public static class VietQrHelper
    {
        public static string BuildQrUrl(string bankId, string accountNo, string accountName,
                                         decimal amount, string addInfo, string template = "compact2")
        {
            var info = Uri.EscapeDataString(addInfo);
            var name = Uri.EscapeDataString(accountName);
            return $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png" +
                   $"?amount={(int)amount}&addInfo={info}&accountName={name}";
        }
    }
}
