
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Auth.Queries.ForgotPhone
{
    public class ForgotPhoneHandler : IRequestHandler<ForgotPhoneQuery, Result<ForgotPhoneResponseDto>>
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

        public async Task<Result<ForgotPhoneResponseDto>> Handle(ForgotPhoneQuery request, CancellationToken cancellationToken)
        {
            var nationalId = request.NationalId.Trim();

            // __ Fetching Patient From DB According to His NID __ //
            var patient = await _uow.Repository<Patient>().GetFirstOrDefaultAsync(p => p.IdentityNumber == nationalId);

            // __ If Not Found __ //
            if (patient is null)
            {
                return Result.Failure<ForgotPhoneResponseDto>("البيانات غير صحيحه");
            }

            // __ Fetching Identity User For The Patient __ //
            var user = await _identity.FindByIdAsync(patient.ApplicationUserId);

            if (user is null)
            {
                return Result.Failure<ForgotPhoneResponseDto>("لا يوجد شخص مسجل بالرقم القومي هذا");
            }

            // __ Checking phone number __ //
            var phoneNumber = patient.PhoneNumber;

            if (phoneNumber is null)
            {
                return Result.Failure<ForgotPhoneResponseDto>("لا يوجد رقم مسجل");
            }

            // __ Masking Phone Number __ //
            var masked = MaskPhoneNumber(phoneNumber);

            // __ Return Masked Phone Number
            return Result.Success<ForgotPhoneResponseDto>
                (
                   new ForgotPhoneResponseDto
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
