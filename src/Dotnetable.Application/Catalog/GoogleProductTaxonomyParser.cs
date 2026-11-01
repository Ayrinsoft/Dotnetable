namespace Dotnetable.Application.Catalog;

/// <summary>One row of Google's product taxonomy file (<c>id - Segment &gt; Segment</c>).</summary>
public sealed record GoogleProductTaxonomyRow(
    int GoogleId,
    string FullPath,
    string Name,
    int? ParentGoogleId,
    byte Depth);

/// <summary>
/// Parses https://www.google.com/basepages/producttype/taxonomy-with-ids.en-US.txt
/// Comment lines (starting with #) are ignored. Parent ids are resolved from the path text.
/// </summary>
public static class GoogleProductTaxonomyParser
{
    public static IReadOnlyList<GoogleProductTaxonomyRow> Parse(string text)
    {
        var parsed = new List<(int Id, string Path)>();
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim().TrimEnd('\r');
            if (line.Length == 0 || line[0] == '#') continue;
            var dash = line.IndexOf(" - ", StringComparison.Ordinal);
            if (dash <= 0) continue;
            if (!int.TryParse(line[..dash].Trim(), out var id)) continue;
            var path = line[(dash + 3)..].Trim();
            if (path.Length == 0) continue;
            parsed.Add((id, path));
        }

        var idByPath = new Dictionary<string, int>(parsed.Count, StringComparer.Ordinal);
        foreach (var row in parsed)
            idByPath[row.Path] = row.Id;

        var result = new List<GoogleProductTaxonomyRow>(parsed.Count);
        foreach (var row in parsed)
        {
            var segments = row.Path.Split(" > ", StringSplitOptions.None | StringSplitOptions.TrimEntries);
            var name = segments.Length == 0 ? row.Path : segments[^1];
            int? parentId = null;
            if (segments.Length > 1)
            {
                var parentPath = string.Join(" > ", segments[..^1]);
                if (idByPath.TryGetValue(parentPath, out var pid))
                    parentId = pid;
            }

            var depth = (byte)Math.Clamp(segments.Length, 1, 255);
            result.Add(new GoogleProductTaxonomyRow(row.Id, row.Path, name, parentId, depth));
        }

        return result;
    }
}
