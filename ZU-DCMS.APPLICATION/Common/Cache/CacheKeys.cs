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
        public static string StudentsPage(int page, int size, string? search, string? sort, bool desc, int version) => $"student:page:{page}:{size}:{search}:{sort}:{desc}:{version}"; 
        
        // __ Student Progress Related __ //
        public static string StudentProgress(int studentId, int termId) => $"student:progress:{studentId}:{termId}"; 
        public static string StudentRequirements(int studentId, int termId) => $"student:requirement:{studentId}:{termId}";
        
        // __ Cases Related __ //
        public static string StudentCases(int studentId) => $"student:cases:{studentId}";
        public static string CaseById(int caseId) => $"case:{caseId}";
        
        // __ Diagnosis Related __ //
        public static string AvailableStudents(int clinicId) => $"students:available:{clinicId}";
        public static string DiagnosisTypes(int clinicId) => $"clinic:diagnosis_types:{clinicId}";
        public static string Procedures(int clinicId) => $"clinic:procedures:{clinicId}";
       
        // __ Booking Related __ //
        public static string PatientBookingsPage(int patientId, int page, int size, int version) => $"patient:bookings:{patientId}:{page}:{size}:{version}";
        public static string PatientBookingsVersion(int patientId) => $"patient:bookings:version:{patientId}";

        // __ Dashboard Related __ //
        public const string DailyMetrics = "dashboard:daily_metrics";

        // __ Staff Related __ //
        public static string StaffUsersPage(int page, int size, string role, string search, string? sort, bool desc, int version) => $"staff:users:{page}:{size}:{role}:{search}:{sort}:{desc}:{version}";
        public static string StaffUsersVersion => "staff:users:version";
        public static string StaffUserByUserId(string userId) => $"staff:user:{userId}";
    }
}