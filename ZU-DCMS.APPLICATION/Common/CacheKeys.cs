using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    // This class defines standardized cache keys for various types of data in the application.
    public static class CacheKeys
    {
        // Static Keys
        public const string Clinics = "clinics";
        public const string SystemConfigs = "system_configs";

        // Dynamic Keys
        public static string AvailableStudents(int clinicId)
            => $"available_students_{clinicId}";
    }
}
