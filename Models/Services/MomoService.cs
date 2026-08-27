using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ShopVanPhongPham.Models.Interfaces;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ShopVanPhongPham.Models.Services
{
    public class MomoService : IMomoService
    {
        private readonly MomoOptionModel _options;

        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options.Value;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            string requestId = Guid.NewGuid().ToString();

            // Ghép timestamp để orderId luôn duy nhất (khóa Sandbox dùng chung, dễ trùng)
            string orderId = $"{model.OrderId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            string orderInfo = model.OrderInfo;
            string amount = ((long)model.Amount).ToString();
            string extraData = "";
            string requestType = "payWithMethod";

            string rawSignature =
                $"accessKey={_options.AccessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&ipnUrl={_options.NotifyUrl}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={_options.PartnerCode}" +
                $"&redirectUrl={_options.ReturnUrl}" +
                $"&requestId={requestId}" +
                $"&requestType={requestType}";

            string signature = ComputeHmacSha256(rawSignature, _options.SecretKey);

            var requestData = new
            {
                partnerCode = _options.PartnerCode,
                partnerName = "ShopVanPhongPham",
                storeId = "ShopVanPhongPhamStore",
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl = _options.ReturnUrl,
                ipnUrl = _options.NotifyUrl,
                lang = "vi",
                extraData,
                requestType,
                signature
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = WebRequest.GetSystemWebProxy()
            };
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new StringContent(
                JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_options.MomoApiUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(responseBody)
                   ?? new MomoCreatePaymentResponseModel { ResultCode = -1, Message = "Không đọc được phản hồi từ Momo" };
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            return new MomoExecuteResponseModel
            {
                OrderId = collection["orderId"].ToString(),
                Success = collection["resultCode"] == "0",
                Message = collection["message"].ToString(),
                Amount = double.TryParse(collection["amount"], out var amt) ? amt : 0,
                OrderInfo = collection["orderInfo"].ToString()
            };
        }

        private static string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}