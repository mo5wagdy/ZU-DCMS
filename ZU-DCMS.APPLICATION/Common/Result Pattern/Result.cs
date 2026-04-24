
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
        public string Error => Errors.FirstOrDefault() ?? string.Empty;
        public List<string> Errors { get; }

        protected Result(bool isSuccess, IEnumerable<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors.ToList();
        }


        // ____________ Factory Methods ____________ //
        public static Result Success() => new(true, Enumerable.Empty<string>());

        public static Result Failure(string error) => new(false, new[] { error });

        public static Result Failure(IEnumerable<string> errors) => new(false, errors);

        public static Result<T> Success<T>(T value) => new(value, true, Enumerable.Empty<string>());

        public static Result<T> Failure<T>(string error) => new(default!, false, new[] { error });

        public static Result<T> Failure<T>(IEnumerable<string> errors) => new(default!, false, errors);
    }

    /// <summary>
    /// Represents the outcome of an operation that returns a value.
    /// </summary>
    public class Result<T> : Result
    {
        private readonly T _value;

        protected internal Result(T value, bool isSuccess, IEnumerable<string> errors) : base(isSuccess, errors)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value. Only call if IsSuccess is true.
        /// </summary>
        public T Value => IsSuccess? _value : throw new InvalidOperationException("Cannot access Value of a failed Result.");
    }
}
