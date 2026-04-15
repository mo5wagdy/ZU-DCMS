
namespace ZU_DCMS.APPLICATION.Common
{
    // __ This class defines standardized cache keys for various types of data in the application. __ //
    public static class CacheKeys
    {
        // ________ Long Duration ________ //
        public const string Clinics = "clinics";
        public const string SystemConfigs = "system_configs";
        public const string SessionConfig = "session_config";


        // ________ Medium Duration ________ //
        public static string DiagnosisTypes(int clinicId) => $"diagnosis_types_{clinicId}";
        public static string Procedures(int clinicId) => $"procedures_{clinicId}";

        // ________ Short Duration ________ // => When a student is assigned or finished
        public static string AvailableStudents(int clinicId) => $"available_students_{clinicId}";
        public static string SessionStatus(int sessionId) => $"session_status_{sessionId}"; // => Changes every booking

        // ________ In every change in requirements ________ //
        public static string StudentProgress(int studentId) => $"student_progress_{studentId}";
        public static string StudentRequirements(int studentId, int termId) => $"student_requirement{studentId}_{termId}";
    }
}

