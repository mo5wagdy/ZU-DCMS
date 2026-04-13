
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Payment;
using ZU_DCMS.APPLICATION.Services.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    /*
     * Service for validating Fawry payment callback signatures
     * This service computes the expected signature based on the callback data and a secret key,
     * and compares it to the signature provided in the callback to ensure that the callback is authentic and has not been tampered with.
     */
    public class FawrySignatureService : IFawrySignatureService
    {
        private readonly string _secret;

        public FawrySignatureService(IConfiguration config)
        {
            _secret = config["Fawry:SecretKey"]!;
        }

        public bool IsValid(FawryCallbackDto dto)
        {
            // __ Construct the raw string by concatenating the payment code, amount, gateway reference, and secret key. __ //
            var raw = $"{dto.PaymentCode}{dto.Amount}{dto.GatewayReference}{_secret}";

            // __ Compute the SHA256 hash of the raw string. __ //
            var hash = ComputeSha256(raw);

            // __ Compare the computed hash with the signature provided in the callback DTO. __ //
            return hash.Equals(dto.Signature, StringComparison.OrdinalIgnoreCase);
        }

        private static string ComputeSha256(string input)
        {
            // __ Create a new instance of the SHA256 hash algorithm and compute the hash of the input string. __ //
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

            // __ Convert the hash bytes to a hexadecimal string and return it. __ //
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
