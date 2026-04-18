using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateConfig
{
    public class UpdateConfigHandler
    {
        private readonly IUnitOfWork _uow;

        public UpdateConfigHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result> Handle(UpdateConfigCommand command)
        {
            var config = await _uow.Repository<SystemConfig>().GetFirstOrDefaultAsync(c => c.Key == command.Key);

            if (config is null)
                return Result.Failure("الإعداد غير موجود");

            config.Value = command.Value.Trim();
            config.UpdatedByAdminId = command.AdminId;
            config.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<SystemConfig>().Update(config);
           
            await _uow.SaveChangesAsync(command.AdminId);

            return Result.Success();
        }
    }
}
