using Microsoft.EntityFrameworkCore;

namespace Application.Database;

public sealed class ReadDbContext(WriteDbContext context)
{
    public IQueryable<TResult> SqlQuery<TResult>(FormattableString sql) => context.Database.SqlQuery<TResult>(sql);
}
