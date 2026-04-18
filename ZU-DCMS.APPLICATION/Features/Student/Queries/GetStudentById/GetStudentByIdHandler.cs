using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Student.Queries.GetStudentById
{
    public class GetStudentByIdHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetStudentByIdHandler> _logger;

        public GetStudentByIdHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetStudentByIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery query)
        {
            var studentId = query.StudentId;

            _logger.LogInfo("Fetching student with ID {StudentId}.", studentId);

            var student = await _uow.Repository<Domain.Entities.Student>().GetFirstOrDefaultAsync
                (
                    st => st.Id == studentId,
                    true,
                    s => s.TermRequirements.Where(tr => tr.Term.IsActive),
                    s => s.CaseAssignments.Where(ca => ca.Clinic.IsActive)
                );

            if (student is null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found.", studentId);
              
                return Result.Failure<StudentDto>("Student not found.");
            }

            _logger.LogInfo("Student with ID {StudentId} found. Mapping to DTO.", studentId);

            return Result.Success(_mapper.Map<StudentDto>(student));
        }
    }
}
