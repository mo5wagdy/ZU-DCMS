using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    /* 
     * Constants for configuration keys used in the application.
     * This class provides a centralized place to manage all configuration keys,
     * ensuring consistency and reducing the risk of typos. 
     */
    public static class ConfigKeys
    {
        public const string MaxDailyPatients = "MAX_DAILY_PATIENTS";
        public const string MaxNewPerSession = "MAX_NEW_PER_SESSION";
        public const string MaxFollowUpPerSession = "MAX_FOLLOWUP_PER_SESSION";
        public const string DiagnosisFee = "DIAGNOSIS_FEE";
        public const string SessionTimes = "SESSION_TIMES";
        public const string WorkingDays = "WORKING_DAYS";
    }
}
