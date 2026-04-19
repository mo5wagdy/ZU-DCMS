
namespace ZU_DCMS.APPLICATION.Exceptions
{
    // __ Exception for Forbidden Access __ //
    public class ForbiddenException : Exception
    {
        // __ Constructor makes the message of the exception with a default message __ //
        public ForbiddenException() : base("You do not have permission to access this resource") { }
    }
}
