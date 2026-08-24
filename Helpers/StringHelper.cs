using System.Globalization;
using System.Text;

namespace ShopVanPhongPham.Helpers
{
    public static class StringHelper
    {
        
        public static string RemoveDiacritics(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString()
                     .Replace('đ', 'd').Replace('Đ', 'D')   
                     .Normalize(NormalizationForm.FormC)
                     .ToLowerInvariant();
        }
    }
}
