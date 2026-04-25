namespace ZU_DCMS.API.Common
{
    /// <summary>
    /// A standardized generic response wrapper for all API endpoints.
    /// Ensures every response shape is consistent across the application.
    /// </summary>
    /// <typeparam name="T">The Type of the data returned in the response.</typeparam>
    public class ApiResponse<T>
    {
        // Indicates whether the request was processed successfully.
        public bool IsSuccess { get; set; }
        
        // Additional information about the response (e.g., a success confirmation or an error summary).
        public string Message { get; set; } = string.Empty;
        
        // The payload/data of the response to be sent back.
        public T? Data { get; set; }

        // A list of error messages, populated when the request fails.
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Creates a successful response containing data and an optional message.
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "") => new() 
        { 
            IsSuccess = true, 
            Data = data, 
            Message = message 
        };

        /// <summary>
        /// Creates a failure response containing a list of error messages and a descriptive message.
        /// </summary>
        public static ApiResponse<T> Failure(IEnumerable<string> errors, string message) => new() 
        { 
            IsSuccess = false, 
            Errors = errors?.ToList() ?? new List<string>(),
            Message = message
        };

        /// <summary>
        /// Helper to create a failure response containing a single error message and a descriptive message.
        /// </summary>
        public static ApiResponse<T> Failure(string error, string message) => new() 
        { 
            IsSuccess = false, 
            Errors = new List<string> { error },
            Message = message
        };
    }
}
