using System.Linq.Expressions;
using System.Reflection;

namespace Dotnetable.Infrastructure.Extensions;

/// <summary>
/// Applies a "Column DIR, Column DIR" sort string to an <see cref="IQueryable{T}"/> as a
/// translatable EF expression. Unknown column names are ignored, so the (whitelisted-by-reflection)
/// input cannot be used to inject arbitrary expressions.
/// </summary>
public static class QueryableSortExtensions
{
    public static IQueryable<T> ApplyOrderBy<T>(
        this IQueryable<T> source, string? orderBy, string fallbackProperty, bool fallbackDescending = false)
    {
        var clauses = Parse(orderBy);
        if (clauses.Count == 0)
            clauses.Add((fallbackProperty, fallbackDescending));

        IOrderedQueryable<T>? ordered = null;
        foreach (var (propName, descending) in clauses)
        {
            var prop = typeof(T).GetProperty(
                propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop is null) continue;

            var param = Expression.Parameter(typeof(T), "x");
            var selector = Expression.Lambda(Expression.Property(param, prop), param);

            var method = ordered is null
                ? (descending ? "OrderByDescending" : "OrderBy")
                : (descending ? "ThenByDescending" : "ThenBy");

            var call = Expression.Call(
                typeof(Queryable), method, new[] { typeof(T), prop.PropertyType },
                (ordered ?? source).Expression, Expression.Quote(selector));

            ordered = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }

        // If nothing matched (all unknown), still guarantee a deterministic order for paging.
        if (ordered is null)
        {
            var prop = typeof(T).GetProperty(
                fallbackProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop is not null)
            {
                var param = Expression.Parameter(typeof(T), "x");
                var selector = Expression.Lambda(Expression.Property(param, prop), param);
                var call = Expression.Call(
                    typeof(Queryable), fallbackDescending ? "OrderByDescending" : "OrderBy",
                    new[] { typeof(T), prop.PropertyType }, source.Expression, Expression.Quote(selector));
                ordered = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
            }
        }

        return ordered ?? source;
    }

    private static List<(string Prop, bool Desc)> Parse(string? orderBy)
    {
        var result = new List<(string, bool)>();
        if (string.IsNullOrWhiteSpace(orderBy)) return result;

        foreach (var part in orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;
            var desc = tokens.Length > 1 && tokens[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
            result.Add((tokens[0], desc));
        }
        return result;
    }
}
