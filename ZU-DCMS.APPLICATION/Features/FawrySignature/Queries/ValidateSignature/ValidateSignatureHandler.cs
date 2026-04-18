using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace ZU_DCMS.APPLICATION.Features.FawrySignature.Queries.ValidateSignature
{
    public class ValidateSignatureHandler
    {
        private readonly string _secret;

        public ValidateSignatureHandler(IConfiguration config)
        {
            _secret = config["Fawry:SecretKey"]!;
        }

        public bool Handle(ValidateSignatureQuery query)
        {
            var dto = query.Dto;
            
            var raw = $"{dto.PaymentCode}{dto.Amount}{dto.GatewayReference}{_secret}";

            var hash = ComputeSha256(raw);

            return hash.Equals(dto.Signature, StringComparison.OrdinalIgnoreCase);
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
