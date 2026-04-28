using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Token;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Common.Token
{
    // __ The ITokenService interface defines the contract for token management, including generating access and refresh tokens, refreshing tokens, and revoking refresh tokens. __ //
    public interface ITokenService
    {
        Task<TokenResult> GenerateAsync(string userId); // => Generates access and refresh tokens for a given user ID.
        Task<TokenResult> RefreshAsync(string refreshToken); // => Validates the provided refresh token, generates new access and refresh tokens, and updates the database accordingly.  
        Task RevokeAsync (RefreshToken stored); // => Revokes the specified refresh token by marking it as revoked in the database, preventing its future use for token refreshing.
        Task<bool> RevokeByTokenAsync(string token); // => Revokes a token by its string value.
    }
}
