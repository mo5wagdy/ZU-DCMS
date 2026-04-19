using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Student.Queries.GetStudentByUserId
{
    public class GetStudentByUserIdHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetStudentByUserIdHandler> _logger;

        public GetStudentByUserIdHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetStudentByUserIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<StudentDto>> Handle(GetStudentByUserIdQuery query)
        {
            var userId = query.UserId;

            _logger.LogInfo("Fetching student with User ID {UserId}.", userId);

            var student = await _uow.Repository<Domain.Entities.Student>().GetFirstOrDefaultAsync
                (
                    st => st.ApplicationUserId == userId,
                    true,
                    s => s.TermRequirements.Where(tr => tr.Term.IsActive),
                    s => s.CaseAssignments.Where(ca => ca.Clinic.IsActive)
                );

            if (student is null)
            {
                _logger.LogWarning("Student with User ID {UserId} not found.", userId);
               
                return Result.Failure<StudentDto>("Student not found.");
            }

            _logger.LogInfo("Student with User ID {UserId} found. Mapping to DTO.", userId);

            return Result.Success(_mapper.Map<StudentDto>(student));
        }
    }
}
