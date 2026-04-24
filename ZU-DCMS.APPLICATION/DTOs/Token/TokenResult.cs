using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.DTOs.Token
{
    // _________________________ Token Result _________________________ //
    public class TokenResult : Result<TokenData>
    {
        private TokenResult(TokenData value, bool isSuccess, IEnumerable<string> errors) : base(value, isSuccess, errors) { }

        public static TokenResult Success(TokenData data) => new(data, true, []);

        public static TokenResult Fail(IEnumerable<string> errors) => new(default!, false, errors);
    }

    // _________________________ Token Data _________________________ //
    public class TokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
