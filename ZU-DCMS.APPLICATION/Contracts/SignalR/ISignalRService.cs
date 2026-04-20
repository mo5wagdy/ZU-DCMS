namespace ZU_DCMS.APPLICATION.Contracts.SignalR
{
    // __ This interface defines methods for sending real-time updates and notifications using SignalR. __ //
    public interface ISignalRService
    {
        Task SendDashboardUpdateAsync(string eventName, object data); // => Send Dashboard Updates to Admin
        Task SendToUserAsync(string userId, string eventName, object data); // => Send Notification to a specific user
        Task SendToRoleAsync(string role, string eventName, object data); // => Send Notification to a specific role
    }
}
