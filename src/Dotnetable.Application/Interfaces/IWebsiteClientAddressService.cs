using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Result of an attempt to add a new address for a customer.</summary>
public enum AddressSaveResult
{
    Success,
    LimitReached,
    NotFound,
}

/// <summary>
/// Manages a website customer's saved addresses (<see cref="WebsiteClientAddress"/>). Every operation
/// is scoped to the owning <see cref="WebsiteClient.WebsiteClientID"/> so a customer (or admin acting
/// on their behalf) can never read or modify another customer's addresses. Each customer may hold at
/// most <see cref="AppConstants.MaxClientAddresses"/> addresses.
/// </summary>
public interface IWebsiteClientAddressService
{
    Task<List<WebsiteClientAddress>> GetByClientIdAsync(int clientId, CancellationToken ct = default);

    /// <summary>Returns the address only when it belongs to <paramref name="clientId"/>.</summary>
    Task<WebsiteClientAddress?> GetByIdAsync(int addressId, int clientId, CancellationToken ct = default);

    /// <summary>Adds a new address. Fails with <see cref="AddressSaveResult.LimitReached"/> once the
    /// customer already has <see cref="AppConstants.MaxClientAddresses"/> saved addresses.</summary>
    Task<AddressSaveResult> CreateAsync(WebsiteClientAddress address, CancellationToken ct = default);

    /// <summary>Updates an existing address; fails with <see cref="AddressSaveResult.NotFound"/> when it
    /// doesn't exist or doesn't belong to <paramref name="address"/>'s <see cref="WebsiteClientAddress.WebsiteClientID"/>.</summary>
    Task<AddressSaveResult> UpdateAsync(WebsiteClientAddress address, CancellationToken ct = default);

    Task<bool> DeleteAsync(int addressId, int clientId, CancellationToken ct = default);

    /// <summary>Marks one address as the customer's default and clears the flag on all others.</summary>
    Task<bool> SetDefaultAsync(int addressId, int clientId, CancellationToken ct = default);
}
