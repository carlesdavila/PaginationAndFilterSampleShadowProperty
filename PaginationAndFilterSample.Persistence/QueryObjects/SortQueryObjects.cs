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

        private static readonly MethodInfo ThenByMethod =
            typeof(Queryable).GetMethods().Single(method =>
                method.Name == "ThenBy" && method.GetParameters().Length == 2);

        private static readonly MethodInfo OrderByDescendingMethod =
            typeof(Queryable).GetMethods().Single(method =>
                method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        public static IQueryable<T> MultiColumnOrderBy<T>(this IQueryable<T> query, string orderByExpression)
        {
            var propertiesToSort = orderByExpression.Split(',').Select(p => p.Trim()).ToList();

            var firstProperty = propertiesToSort.FirstOrDefault();
            if (string.IsNullOrEmpty(firstProperty))
            {
                return query;
            }

            if (PropertyExists<T>(firstProperty))
            {
                query = query.OrderByProperty(firstProperty);
            }else if (firstProperty.Equals(ShadowPropertyConstants.LastModified, StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderBy(c => EF.Property<DateTime>(c, ShadowPropertyConstants.LastModified));
            }


            foreach (var property in propertiesToSort.Skip(1))
            {
                if (PropertyExists<T>(property))
                {
                    query = query.ThenByProperty(property);
                }
                else
                {
                    if (property.Equals(ShadowPropertyConstants.LastModified, StringComparison.OrdinalIgnoreCase))
                    {
                        query = ((IOrderedQueryable<T>)query).ThenBy(c => EF.Property<DateTime>(c, ShadowPropertyConstants.LastModified));
                    }
                }
            }
            return query;
        }


        private static IQueryable<T> ThenByProperty<T>(
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
                ThenByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
            var ret = genericMethod.Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)ret;
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