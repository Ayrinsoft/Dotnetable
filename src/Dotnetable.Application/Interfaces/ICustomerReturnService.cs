using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public interface ICustomerReturnService
{
    Task<PagedResult<CustomerReturnDto>> GetPagedAsync(int websiteId, int? clientId, byte? status, GridQuery query, CancellationToken ct = default);
    Task<CustomerReturnDto?> GetByIdAsync(int returnRequestId, int? clientId = null, CancellationToken ct = default);
    Task<ReturnEligibilityDto> GetEligibilityAsync(int orderId, int clientId, CancellationToken ct = default);

    Task<(bool Success, string? Error, CustomerReturnDto? Request)> CreateAsync(
        int websiteId, int clientId, int orderId,
        CustomerReturnReason reason, string? reasonNote, string? description,
        string? shipMethod, IReadOnlyList<CustomerReturnLineInput> lines,
        IReadOnlyList<int>? photoFileIds,
        ReturnShippingPayer shippingPayer = ReturnShippingPayer.Unset,
        bool acceptExpiredWindow = false,
        CancellationToken ct = default);

    Task<(bool Success, string? Error)> AttachPhotoAsync(int returnRequestId, int clientId, int fileRecordId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> ApproveAsync(
        int returnRequestId, ReturnShippingPayer shippingPayer, string? reviewNote,
        IReadOnlyList<CustomerReturnLineApproval> lineApprovals, int memberId,
        decimal returnShippingCost = 0, int? receivedWarehouseId = null, CancellationToken ct = default);

    Task<(bool Success, string? Error)> RejectAsync(int returnRequestId, string? reviewNote, int memberId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> SubmitShipmentAsync(
        int returnRequestId, int clientId, string? shipMethod, string trackingCode, CancellationToken ct = default);

    Task<(bool Success, string? Error)> UpdateTrackingAsync(
        int returnRequestId, int clientId, string trackingCode, string? shipMethod, CancellationToken ct = default);

    Task<(bool Success, string? Error)> MarkReceivedAsync(
        int returnRequestId, int memberId, int? warehouseId = null,
        IReadOnlyList<CustomerReturnLineReceive>? lines = null,
        decimal? returnShippingCost = null, CancellationToken ct = default);
    Task<(bool Success, string? Error)> MarkCompletedAsync(int returnRequestId, int memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CancelAsync(int returnRequestId, int? clientId, int? memberId, string? note, CancellationToken ct = default);
}
