namespace ZU_DCMS.API.Common
{
    // __ API response wrapper to standardize the structure of responses sent to clients. __ //
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; } // => Indicates whether the API call was successful or not.
        public string Message { get; set; } = string.Empty; // => A message providing additional information about the response, such as success confirmation or error details.
        public T? Data { get; set; } // => The actual data being returned in the response, which can be of any type specified by the generic parameter T.

        // __ A collection of error messages that can be populated when the API call fails, allowing for multiple errors to be returned in a structured way. __ //
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        // __ Static factory methods to create standardized success and failure responses. __ //
        public static ApiResponse<T> Ok(T data, string message = "") => new() { IsSuccess = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string error) => new() {IsSuccess = false, Errors = new[] { error }};

        // __ Overloaded method to handle multiple errors in case of failure. __ //
        public static ApiResponse<T> Fail(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors };
    }
}
