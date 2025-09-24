using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Data;

public static class SoftDeleteQueryFilter
{
    public static void ApplySoftDeleteFilter<T>(this ModelBuilder modelBuilder, Expression<Func<T, bool>> filter)
        where T : class
    {
        modelBuilder.Entity<T>().HasQueryFilter(filter);
    }

    public static IQueryable<T> WithSoftDeleteFilter<T>(this IQueryable<T> query, bool includeDeleted = false)
        where T : class
    {
        if (includeDeleted)
        {
            return query.IgnoreQueryFilters();
        }
        return query;
    }
}
