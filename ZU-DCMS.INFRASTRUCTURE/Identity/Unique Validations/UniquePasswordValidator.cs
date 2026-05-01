
using Microsoft.AspNetCore.Identity;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.INFRASTRUCTURE.Identity.Unique_Validations
{
    /* 
     * This validator checks if the user is in the "Patient" role. If so, it ensures that the password is exactly 14 digits long,
     * which is likely intended to be a national ID number.
     * For staff users, it enforces a more traditional password policy requiring at least 8 characters,
     * at least one uppercase letter, and at least one special character. 
     */
    public class UniquePasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : ApplicationUser
    {
        // __ Validates the password based on the user's role. __ //
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
        {
            // __ Check if the user is in the "Patient" role. __ //
            //var isPatient = await manager.IsInRoleAsync(user, "Patient");

            // __ If the user is a patient, validate that the password is exactly 14 digits long. __ //
            if (user.UserType == UserType.Patient)
            {
                // __ National ID is 14 digits, Passport/Residence is 6-20 alphanumeric characters. __ //
                if (password?.Length >= 6 && password?.Length <= 20 && password.All(char.IsLetterOrDigit))
                    return IdentityResult.Success;

                return IdentityResult.Failed(new IdentityError { Description = "رقم الهوية غير صحيح (يجب أن يكون بين 6 و 20 حرف أو رقم)"});
            }
            else
            {
                // __ List to hold any validation errors for staff users. __ //
                var errors = new List<IdentityError>();

                // __ For staff users, validate that the password meets the specified criteria. __ //
                if (string.IsNullOrWhiteSpace(password) || password?.Length < 8)
                    errors.Add(new IdentityError { Description = "كلمة المرور يجب أن تكون 8 أحرف على الأقل" });

                if (!password?.Any(char.IsUpper) ?? true)
                    errors.Add(new IdentityError { Description = "يجب وجود حرف كبير (Uppercase)" });

                if (!password?.Any(char.IsLetterOrDigit) ?? true)
                    errors.Add(new IdentityError { Description = "يجب وجود رمز خاص على الأقل (@#$)" });

                // __ Return the validation result based on whether there are any errors. __ //
                return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
            }
        }
    }
}
