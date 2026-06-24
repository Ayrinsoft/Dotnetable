using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Resolves a registered <see cref="IFileStorageProvider"/> by its <see cref="StorageProviderType"/>.</summary>
public sealed class FileStorageProviderRegistry : IFileStorageProviderRegistry
{
    private readonly IEnumerable<IFileStorageProvider> _providers;

    public FileStorageProviderRegistry(IEnumerable<IFileStorageProvider> providers) => _providers = providers;

    public IFileStorageProvider Get(StorageProviderType provider) =>
        _providers.FirstOrDefault(p => p.Provider == provider)
        ?? throw new NotSupportedException($"No storage provider registered for '{provider}'.");
}
