using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Token.Commands.RevokeToken
{
    public class RevokeTokenHandler
    {
        private readonly IUnitOfWork _uow;

        public RevokeTokenHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Task Handle(RevokeTokenCommand command)
        {
            var stored = command.Stored;

            if (stored == null || !stored.IsActive) return Task.CompletedTask;

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            _uow.Repository<Domain.Entities.RefreshToken>().Update(stored);

            return Task.CompletedTask;
        }
    }
}
