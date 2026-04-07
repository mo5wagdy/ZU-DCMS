using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    // __ This class defines standardized cache keys for various types of data in the application. __ //
    public static class CacheKeys
    {
        // ________ Long Duration ________ //
        public const string Clinics = "clinics";
        public const string SystemConfigs = "system_configs";

        // ________ Medium Duration ________ //
        public static string DiagnosisTypes(int clinicId)
            => $"diagnosis_types_{clinicId}";

        public static string Procedures(int clinicId)
            => $"procedures_{clinicId}";

        // ________ Short Duration ________ // => When a student is assigned or finished
        public static string AvailableStudents(int clinicId)
            => $"available_students_{clinicId}";

        public static string SessionStatus(int sessionId) // => Changes every booking
            => $"session_status_{sessionId}";

        // ________ In every change in requirements ________ //
        public static string StudentProgress(int studentId)
            => $"student_progress_{studentId}";
    }
}

