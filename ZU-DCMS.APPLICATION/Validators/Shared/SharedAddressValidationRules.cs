using FluentValidation;

namespace ZU_DCMS.APPLICATION.Validators.Shared
{
    /*
     * Validation rules for shared address fields,
     * ensuring they follow the format "Country,Province" if provided.
     */
    public static class SharedAddressValidationRules
    {
        // __ Validates that the address is either empty or follows the "Country,Province" format. __ //
        public static IRuleBuilderOptions<T, string?> ValidAddress<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                  .Must(address =>
                  {
                      if (string.IsNullOrWhiteSpace(address)) return true;
                      var parts = address.Split(',');
                      return parts.Length == 2 &&
                             parts.All(p => !string.IsNullOrWhiteSpace(p.Trim()));
                  })
                  .WithMessage("العنوان لازم يكون: الدولة,المحافظة");
        }
    }
}
