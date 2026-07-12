namespace Dotnetable.Domain.Enums;

/// <summary>
/// Anchor point of a watermark overlay relative to the base image.
/// Stored as a TINYINT in <see cref="Entities.WebsiteWatermarkSetting.Position"/>.
/// </summary>
public enum WatermarkPosition : byte
{
    TopLeft = 1,
    TopCenter = 2,
    TopRight = 3,
    MiddleLeft = 4,
    Center = 5,
    MiddleRight = 6,
    BottomLeft = 7,
    BottomCenter = 8,
    BottomRight = 9,
}
