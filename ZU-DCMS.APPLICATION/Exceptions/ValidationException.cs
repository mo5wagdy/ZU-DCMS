
using FluentValidation.Results;

namespace ZU_DCMS.APPLICATION.Exceptions
{
    // __ Exception for Validation Errors __ //
    public class ValidationException : Exception
    {
        // __ Property to hold the list of validation failures __ //
        public IDictionary<string, string[]> Errors { get; }
        // __ Constructor takes a list of validation failures and sets the message to a default value __ //
        public ValidationException(IEnumerable<ValidationFailure> failures)
        {
            Errors = failures
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }
    }
}
