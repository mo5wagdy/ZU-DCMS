namespace ZU_DCMS.APPLICATION.Features.Token.Commands.RevokeToken
{
    public class RevokeTokenCommand
    {
        public Domain.Entities.RefreshToken Stored { get; set; } = null!;
    }
}
