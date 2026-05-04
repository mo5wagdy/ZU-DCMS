using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements
{
    public record SetTermRequirementsCommand(int AcademicYear, int TermId, List<SetRequirementDto> Requirements, string AdminId) : IRequest<Result>;
}
