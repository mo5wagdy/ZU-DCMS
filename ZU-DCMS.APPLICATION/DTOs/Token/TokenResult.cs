using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.DTOs.Token
{
    // _________________________ Token Result _________________________ //
    public class TokenResult : Result<TokenData>
    {
        private TokenResult(TokenData value, bool isSuccess, string error) : base(value, isSuccess, error) { }

        public static TokenResult Success(TokenData data) => new(data, true, string.Empty);

        public static TokenResult Fail(string error) => new(default!, false, error);
    }

    // _________________________ Token Data _________________________ //
    public class TokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
