using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    // __ This class defines standardized cache keys for various types of data in the application. __ //
    public static class CacheKeys
    {
        // ____________ Static Keys ____________ //
        public const string Clinics = "clinics";
        public const string SystemConfigs = "system_configs";

        // ____________ Dynamic Keys ____________ //
        public static string AvailableStudents(int clinicId)
            => $"available_students_{clinicId}";
    }
}
