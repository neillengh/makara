using System.Linq.Expressions;

namespace Makara.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// 带条件过滤 + 跳过/取数 + 排序（可选）的查询。
    /// orderByKey：若为空字符串则按默认顺序返回。
    /// </summary>
    Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        int skip = 0,
        int take = 100,
        string? orderByKey = null,
        bool descending = true);

    Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<T> InsertAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
}
