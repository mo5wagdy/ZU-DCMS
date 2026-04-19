
namespace ZU_DCMS.APPLICATION.Exceptions
{
    // __ Exception for Not Found Entity __ //
    public class NotFoundException : Exception
    {
        // __ Constructor makes the message of the exception with the entity name and the key that was not found __ //
        public NotFoundException(string entity, object key) : base($"{entity} With Id ({key}) Not Found") { }
    }
}
