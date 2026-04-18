using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateTerm
{
    public class UpdateTermHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UpdateTermHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<TermDto>> Handle(UpdateTermCommand command)
        {
            var dto = command.Dto;

            var term = await _uow.Repository<Term>().GetByIdAsync(command.TermId);

            if (term is null)
                return Result.Failure<TermDto>("الترم غير موجود");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                term.Name = dto.Name.Trim();

            if (dto.StartDate.HasValue)
                term.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                term.EndDate = dto.EndDate.Value;

            if (dto.RequiredCasesCount.HasValue)
                term.RequiredCasesCount = dto.RequiredCasesCount.Value;

            term.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<Term>().Update(term);
            
            await _uow.SaveChangesAsync(command.AdminId);

            return Result.Success(_mapper.Map<TermDto>(term));
        }
    }
}
