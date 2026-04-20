using System;

namespace ZU_DCMS.APPLICATION.Common.Cache
{
    // __ This class defines standard durations for caching data in the application. __ //
    public static class CacheDuration
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Long = TimeSpan.FromHours(24);
    }
}
