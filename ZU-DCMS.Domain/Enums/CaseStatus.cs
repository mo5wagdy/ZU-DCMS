
namespace ZU_DCMS.Domain.Enums
{
    // __ This enum represents the status of a case, indicating whether it is active, completed, or transferred. __ //
    public enum CaseStatus
    {
        InProgress = 1, 
        Completed = 2,
        PendingReview = 3,
        Approved = 4,
        Rejected = 5
    }
}
