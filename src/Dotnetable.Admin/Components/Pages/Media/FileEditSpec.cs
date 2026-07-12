using Dotnetable.Application.Interfaces;

namespace Dotnetable.Admin.Components.Pages.Media;

/// <summary>Per-file upload options collected from <see cref="MediaEditDialog"/> before the actual upload call.</summary>
public sealed class FileEditSpec
{
    public string? FileName { get; set; }
    public ImageCropRect? Crop { get; set; }
    public int? ResizeWidth { get; set; }
    public int? ResizeHeight { get; set; }
    public bool Grayscale { get; set; }
    public bool? ApplyWatermark { get; set; }

    public bool HasEdits => Crop is not null || ResizeWidth is not null || ResizeHeight is not null || Grayscale || ApplyWatermark is not null;
}
