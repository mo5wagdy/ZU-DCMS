using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetUserById
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<StaffUsersDto>>
    {
        private readonly IIdentityService _identity;
        private readonly IMapper _mapper;

        public GetUserByIdHandler(IIdentityService identity, IMapper mapper)
        {
            _identity = identity;
            _mapper = mapper;
        }

        public async Task<Result<StaffUsersDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            // __ Fetch user by Id __ //
            var user = _identity.FindByIdAsync(query.UserId);

            // __ If null → return null __ //
            if (user is null)
                return Result.Failure<StaffUsersDto>("المستخدم غير موجود");

            // __ Return DTO __ //
            return Result.Success<StaffUsersDto>(_mapper.Map<StaffUsersDto>(user));
        }
    }
}
