namespace Dotnetable.Application.DTOs;

/// <summary>A live work-queue item for the admin dashboard / notifications "tasks" panel.</summary>
public sealed record AdminOpenTask(
    string Key,
    int Count,
    string ActionUrl);
