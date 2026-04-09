using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Common
{
    // __ This class provides a mapping of user roles to their respective dashboard URLs. __ //
    public static class RedirectUrls
    {
        // __ A read-only dictionary that maps user roles to their corresponding dashboard URLs. __ //
        private static readonly IReadOnlyDictionary<string, string> _urls = new Dictionary<string, string>
        {
            [UserRoles.Patient] = "/patient/dashboard",
            [UserRoles.Admin] = "/admin/dashboard",
            [UserRoles.InternDoctor] = "/intern/dashboard",
            [UserRoles.Student] = "/student/dashboard",
        };

        // __ Retrieves the dashboard URL based on the provided user role. If the role is not found, it defaults to the root URL ("/"). __ //
        public static string GetByRole(string role) => _urls.TryGetValue(role, out var url) ? url : "/";
    }
}
