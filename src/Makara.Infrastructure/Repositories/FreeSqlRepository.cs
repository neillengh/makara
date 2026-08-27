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
