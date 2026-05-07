
namespace ZU_DCMS.Domain.Enums
{
    // __ This enum represents the status of a case, indicating whether it is active, completed, or transferred. __ //
    public enum CaseStatus
    {
        InProgress = 1, 
        Completed = 2,
        Transferred = 3,
        PendingReview = 4,
        Approved = 5,
        Rejected = 6,

        // __ Pre-Treatment Assignment Review Statuses __ //
        PendingAssignmentApproval = 7, // __ بانتظار موافقة المعيد على التعيين المبدئي __ //
        EscalatedToSpecialist = 8,     // __ تم تصعيد الحالة لعيادة متخصصة لصعوبتها __ //
        TransferredToIntern = 9        // __ تم تحويل الحالة لعيادة الامتياز فقط __ //
    }
}
