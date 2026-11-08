namespace Dotnetable.Infrastructure.Social;

/// <summary>One X post. Images stay in groups of four. A video is always alone.</summary>
public sealed record XChunk(bool Video, int Start, int Count);

public static class SocialPostPlanner
{
    public const int XImageLimit = 4;
    public const int TelegramAlbum = 10;
    public const int TelegramCaption = 1024;
    public const int TelegramMessage = 4096;
    public const int XText = 280;

    public static IReadOnlyList<XChunk> PlanX(IReadOnlyList<bool> isVideo)
    {
        var result = new List<XChunk>();
        var imageStart = -1;
        var imageCount = 0;
        void Flush()
        {
            if (imageCount == 0) return;
            result.Add(new XChunk(false, imageStart, imageCount));
            imageStart = -1;
            imageCount = 0;
        }

        for (var i = 0; i < isVideo.Count; i++)
        {
            if (isVideo[i])
            {
                Flush();
                result.Add(new XChunk(true, i, 1));
                continue;
            }

            if (imageCount == 0) imageStart = i;
            imageCount++;
            if (imageCount == XImageLimit) Flush();
        }

        Flush();
        return result;
    }

    public static IReadOnlyList<(int Start, int Count)> PlanTelegram(int count)
    {
        var result = new List<(int Start, int Count)>();
        for (var start = 0; start < count; start += TelegramAlbum)
            result.Add((start, Math.Min(TelegramAlbum, count - start)));
        return result;
    }

    public static IReadOnlyList<string> SplitText(string? text, int limit)
    {
        if (string.IsNullOrWhiteSpace(text) || limit < 1) return [];
        var value = text.Trim();
        var parts = new List<string>();
        while (value.Length > limit)
        {
            parts.Add(value[..limit]);
            value = value[limit..];
        }
        if (value.Length > 0) parts.Add(value);
        return parts;
    }
}
