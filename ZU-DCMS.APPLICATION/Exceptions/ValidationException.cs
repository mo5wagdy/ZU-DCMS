using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Exceptions
{
    // __ Exception for Validation Errors __ //
    public class ValidationException : Exception
    {
        // __ Property to hold the list of validation errors __ //
        public IEnumerable<string> Errors { get; }

        // __ Constructor takes a list of validation errors and sets the message to a default value __ //
        public ValidationException(IEnumerable<string> errors) : base("Failed validation")
        {
            Errors = errors;
        }
    }
}
