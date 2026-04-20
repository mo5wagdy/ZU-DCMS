namespace ZU_DCMS.APPLICATION.Common.Cache
{
    // __ This class defines standardized cache keys for various types of data in the application. __ //
    public static class CacheKeys
    {
        // __ Long Term __ //
        public const string Clinics = "clinics";
        public const string SystemConfigs = "system:configs";
        public const string SessionConfig = "session:config";
        public const string StaffUsers = "staff:users";

        // __ Student Related __ //
        public static string StudentById(int studentId) => $"student:{studentId}"; 
        public static string StudentByUserId(string userId) => $"student:user:{userId}"; 
        public static string StudentsPage(int page, int size, string? search, string? sort, int version) => $"student:page:{page}:{size}:{search}:{sort}"; 
        
        // __ Student Progress Related __ //
        public static string StudentProgress(int studentId, int termId) => $"student:progress:{studentId}:{termId}"; 
        public static string StudentRequirements(int studentId, int termId) => $"student:requirement:{studentId}:{termId}";
        
        // __ Cases Related __ //
        public static string StudentCases(int studentId) => $"student:cases:{studentId}";
        public static string CaseById(int caseId) => $"case:{caseId}";
        
        // __ Diagnosis Related __ //
        public static string AvailableStudents(int clinicId) => $"students:available:{clinicId}";
       
        // __ Booking Related __ //
        public static string PatientBookings(int patientId) => $"patient:bookings:{patientId}";

        // __ Staff Related __ //
        public static string StaffUserByUserId(string userId) => $"staff:user:{userId}";
    }
}