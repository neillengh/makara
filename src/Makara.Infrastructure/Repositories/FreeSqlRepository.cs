using System.Linq.Expressions;
using Makara.Core.Interfaces;

namespace Makara.Infrastructure.Repositories;

public class FreeSqlRepository<T> : IRepository<T> where T : class
{
    private readonly IFreeSql _fsql;

    public FreeSqlRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public async Task<T?> GetByIdAsync(string id) =>
        await _fsql.Select<T>().Where("Id = @id", new { id }).FirstAsync();

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _fsql.Select<T>().ToListAsync();

    public async Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        int skip = 0,
        int take = 100,
        string? orderByKey = null,
        bool descending = true)
    {
        var query = _fsql.Select<T>();
        if (predicate is not null)
            query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(orderByKey))
            query = query.OrderBy($"{orderByKey} {(descending ? "DESC" : "ASC")}");

        return await query.Skip(skip).Take(take).ToListAsync();
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _fsql.Select<T>();
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.CountAsync();
    }

    public async Task<T> InsertAsync(T entity)
    {
        await _fsql.Insert<T>().AppendData(entity).ExecuteAffrowsAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await _fsql.Update<T>().SetSource(entity).ExecuteAffrowsAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _fsql.Delete<T>().Where("Id = @id", new { id }).ExecuteAffrowsAsync() > 0;
}
