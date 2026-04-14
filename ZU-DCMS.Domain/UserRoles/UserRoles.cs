using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.UserRoles
{
    /* 
     * This class defines constant string values for different user roles in the application.
     * Used in role-based authorization to control access to various parts of the application based on the user's role.
     * */
    public static class UserRoles
    {
        // معيد - Assistant Lecturer
        // يراجع الحاله بعد ما الطالب يتممها
        public const string Admin = "Admin";
        public const string InternDoctor = "InternDoctor";
        public const string Student = "Student";
        public const string Patient = "Patient";
    }
}
