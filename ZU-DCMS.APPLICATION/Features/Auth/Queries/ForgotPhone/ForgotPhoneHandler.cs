
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Auth.Queries.ForgotPhone
{
    public class ForgotPhoneHandler : IRequestHandler<ForgotPhoneQuery, Result<ForgotPhoneDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;
        public ForgotPhoneHandler
        (
            IUnitOfWork uow,
            IIdentityService identity
        )
        {
            _uow = uow;
            _identity = identity;
        }

        public async Task<Result<ForgotPhoneDto>> Handle(ForgotPhoneQuery query, CancellationToken cancellationToken)
        {
            var nationalId = query.nationalId.Trim();

            // __ Fetching Patient From DB According to His NID __ //
            var patient = await _uow.Repository<Patient>().GetFirstOrDefaultAsync(p => p.IdentityNumber == nationalId);

            // __ If Not Found __ //
            if (patient is null)
            {
                return Result.Failure<ForgotPhoneDto>("البيانات غير صحيحه");
            }

            // __ Fetching Identity User For The Patient __ //
            var user = await _identity.FindByIdAsync(patient.ApplicationUserId);

            if (user is null || string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                return Result.Failure<ForgotPhoneDto>("لا يوجد رقم مسجل");
            }

            // __ Masking Phone Number __ //
            var masked = MaskPhoneNumber(user.PhoneNumber);

            // __ Return Masked Phone Number
            return Result.Success<ForgotPhoneDto>
                (
                   new ForgotPhoneDto
                   {
                      MaskedPhoneNumber = masked
                   }
                );

        }

        // __ Private String Masking Helper For Phone Security __ //
        private static string MaskPhoneNumber( string phoneNumber )
        {
            phoneNumber = phoneNumber.Trim();

            // __ Masking String __ //
            if (phoneNumber.Length < 6)
                return "****";

            // __ Only Show First 4 Numbers And Last 2 __ //
            var first_4 = phoneNumber.Substring(0, 4);
            var last_2 = phoneNumber.Substring(phoneNumber.Length - 2);

            return $"{first_4} **** {last_2}";
        }
    }
}
