namespace Dotnetable.Domain;

/// <summary>
/// General-purpose helper functions shared across the whole solution.
/// </summary>
public static class Tools
{
    /// <summary>
    /// Converts a raw byte (e.g. a database column value) to any byte-backed enum.
    /// </summary>
    public static TEnum ToEnum<TEnum>(this byte value) where TEnum : struct, Enum
        => (TEnum)Enum.ToObject(typeof(TEnum), value);

    /// <summary>
    /// Converts any enum value back to its underlying byte representation.
    /// </summary>
    public static byte ToByte<TEnum>(this TEnum value) where TEnum : struct, Enum
        => Convert.ToByte(value);
}
