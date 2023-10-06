using System.Linq.Expressions;
using System.Reflection;

namespace Dotnetable.Shared.Tools;

public static class LinqExtension
{
    private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out Type resultType) where TEntity : class
    {
        var parameter = Expression.Parameter(typeof(TEntity), "Entity");
        PropertyInfo property;
        Expression propertyAccess;
        if (propertyName.Contains('.'))
        {
            string[] childProperties = propertyName.Split('.');
            property = typeof(TEntity).GetProperty(childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            propertyAccess = Expression.MakeMemberAccess(parameter, property);
            for (int i = 1; i < childProperties.Length; i++)
            {
                property = property.PropertyType.GetProperty(childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
            }
        }
        else
        {
            property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            propertyAccess = Expression.MakeMemberAccess(parameter, property);
        }
        resultType = property.PropertyType;
        return Expression.Lambda(propertyAccess, parameter);
    }

    private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, string fieldName) where TEntity : class
    {
        Type type = typeof(TEntity);
        Type selectorResultType;
        LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
        MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName,
                        new Type[] { type, selectorResultType },
                        source.Expression, Expression.Quote(selector));
        return resultExp;
    }

    public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderBy", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }

    public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderByDescending", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }

    public static IOrderedQueryable<TEntity> ThenBy<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenBy", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }

    public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
    {
        MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenByDescending", fieldName);
        return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
    }

    public static IOrderedQueryable<TEntity> OrderUsingSortExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression) where TEntity : class
    {
        string[] orderFields = sortExpression.Split(',');
        IOrderedQueryable<TEntity> result = null;
        for (int currentFieldIndex = 0; currentFieldIndex < orderFields.Length; currentFieldIndex++)
        {
            string[] expressionPart = orderFields[currentFieldIndex].Trim().Split(' ');
            string sortField = expressionPart[0];
            bool sortDescending = (expressionPart.Length == 2) && (expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase));
            if (sortDescending)
            {
                result = currentFieldIndex == 0 ? source.OrderByDescending(sortField) : result.ThenByDescending(sortField);
            }
            else
            {
                result = currentFieldIndex == 0 ? source.OrderBy(sortField) : result.ThenBy(sortField);
            }
        }
        return result;
    }

}