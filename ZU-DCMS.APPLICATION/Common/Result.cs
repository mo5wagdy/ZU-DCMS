using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    /// <summary>
    /// Represents the outcome of an operation.
    /// Avoids throwing exceptions for expected business failures.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }
        
        protected Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }


        // ____________ Factory Methods ____________ //
        public static Result Success() => new(true, string.Empty);

        public static Result Failure(string error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

        public static Result<T> Failure<T>(string error) => new(default!, false, error);
    }

    /// <summary>
    /// Represents the outcome of an operation that returns a value.
    /// </summary>
    public class Result<T> : Result
    {
        private readonly T _value;

        protected internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value. Only call if IsSuccess is true.
        /// </summary>
        public T Value => IsSuccess? _value : throw new InvalidOperationException("Cannot access Value of a failed Result.");
    }
}
