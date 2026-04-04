using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    public interface ISignalRService
    {
        // Send Dashboard Updates to Admin
        Task SendDashboardUpdateAsync(string eventName, object data);

        // Send Notification to a specific user
        Task SendToUserAsync(string userId, string eventName, object data);

        // Send Notification to a specific role
        Task SendToRoleAsync(string role, string eventName, object data);
    }
}
