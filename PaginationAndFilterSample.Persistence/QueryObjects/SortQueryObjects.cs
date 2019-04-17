using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace PaginationAndFilterSample.Persistence.QueryObjects
{
    public static class SortQueryObjects
    {
        private static readonly MethodInfo OrderByMethod =
            typeof(Queryable).GetMethods().Single(method =>
                method.Name == "OrderBy" && method.GetParameters().Length == 2);

        private static readonly MethodInfo OrderByDescendingMethod =
            typeof(Queryable).GetMethods().Single(method =>
                method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        public static IQueryable<T> MultiColumnOrderBy<T>(this IQueryable<T> query, string orderBy,
            params string[] shadowPropertyList)
        {
            var propertiesToSort = orderBy.Split(',').Select(p => p.Trim()).ToList();

            foreach (var property in propertiesToSort)
            {
                if (PropertyExists<T>(property))
                {
                    query = query.OrderByProperty(property);
                }
                else
                {
                    var sProperty = shadowPropertyList.FirstOrDefault(s => s.Equals(property, StringComparison.OrdinalIgnoreCase));
                    if (sProperty != null)
                    {
                        query = query.OrderBy(c => EF.Property<DateTime>(c, sProperty));
                    }
                }
            }
            return query;
        }

        private static IQueryable<T> OrderByProperty<T>(
            this IQueryable<T> source, string propertyName)
        {
            if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                                                    BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return null;
            }

            var parameterExpression = Expression.Parameter(typeof(T));
            var orderByProperty = Expression.Property(parameterExpression, propertyName);
            var lambda = Expression.Lambda(orderByProperty, parameterExpression);
            var genericMethod =
                OrderByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
            var ret = genericMethod.Invoke(null, new object[] {source, lambda});
            return (IQueryable<T>) ret;
        }

        private static bool PropertyExists<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                                                       BindingFlags.Public | BindingFlags.Instance) != null;
        }

        public static IQueryable<T> OrderByPropertyDescending<T>(
            this IQueryable<T> source, string propertyName)
        {
            if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                                                    BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return null;
            }

            var parameterExpression = Expression.Parameter(typeof(T));
            Expression orderByProperty = Expression.Property(parameterExpression, propertyName);
            var lambda = Expression.Lambda(orderByProperty, parameterExpression);
            var genericMethod =
                OrderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
            var ret = genericMethod.Invoke(null, new object[] {source, lambda});
            return (IQueryable<T>) ret;
        }
    }
}