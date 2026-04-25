using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateTerm
{
    public class CreateTermHandler : IRequestHandler<CreateTermCommand, Result<TermDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateTermHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<TermDto>> Handle(CreateTermCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;

            // __ Prevent multiple active terms __ //
            var hasActiveTerm = await _uow.Repository<Term>().ExistsAsync(t => t.IsActive);

            if (hasActiveTerm)
                return Result.Failure<TermDto>("يوجد ترم نشط بالفعل، أوقفه أولاً");

            // __ Create Term entity __ //
            var term = new Term
            {
                Name = dto.Name.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                RequiredCasesCount = dto.RequiredCasesCount,
                IsActive = false,
                CreatedByAdminId = command.AdminId,
                CreatedAt = DateTime.UtcNow
            };

            // __ Adding to database __ //
            await _uow.Repository<Term>().AddAsync(term);
            
            // __ Saving Changes with admin id for audit trail __ //
            await _uow.SaveChangesAsync(command.AdminId, cancellationToken);

            return Result.Success(_mapper.Map<TermDto>(term));
        }
    }
}