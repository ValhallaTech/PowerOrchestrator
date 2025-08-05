using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace PowerOrchestrator.API.Hubs;

/// <summary>
/// SignalR hub for real-time execution updates
/// </summary>
[Authorize]
public class ExecutionHub : Hub
{
    /// <summary>
    /// Joins a group to receive updates for a specific execution
    /// </summary>
    /// <param name="executionId">The execution ID to monitor</param>
    /// <returns>Task representing the async operation</returns>
    public async Task JoinExecutionGroup(string executionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"execution_{executionId}");
    }

    /// <summary>
    /// Leaves a group to stop receiving updates for a specific execution
    /// </summary>
    /// <param name="executionId">The execution ID to stop monitoring</param>
    /// <returns>Task representing the async operation</returns>
    public async Task LeaveExecutionGroup(string executionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"execution_{executionId}");
    }

    /// <summary>
    /// Joins the general executions group to receive all execution updates
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task JoinExecutionsGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "executions");
    }

    /// <summary>
    /// Leaves the general executions group
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task LeaveExecutionsGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "executions");
    }
}