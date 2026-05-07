using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateConfig
{
    public class UpdateConfigHandler : IRequestHandler<UpdateConfigCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;

        public UpdateConfigHandler(IUnitOfWork uow, IFusionCache cache)
        {
            _uow = uow;
            _cache = cache;
        }

        public async Task<Result> Handle(UpdateConfigCommand command, CancellationToken cancellationToken)
        {
            // __ Fetch config by key __ //
            var config = await _uow.Repository<SystemConfig>().GetFirstOrDefaultAsync(c => c.Key == command.Key);

            // __ If not found → NotFound __ //
            if (config is null)
                return Result.Failure("Setting not found");

            // __ Update value __ //
            config.Value = command.Value.Trim();

            // __ Set UpdatedBy = adminId __ //
            config.UpdatedByAdminId = command.AdminId;
            
            config.UpdatedAt = DateTime.UtcNow;

            // __ SaveChanges __ //
            _uow.Repository<SystemConfig>().Update(config);
           
            await _uow.SaveChangesAsync(command.AdminId, cancellationToken);

            // __ Cache Invalidation __ //
            await _cache.RemoveAsync(CacheKeys.SystemConfigs);

            return Result.Success();
        }
    }
}
