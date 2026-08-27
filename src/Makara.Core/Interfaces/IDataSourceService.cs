using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IDataSourceService
{
    Task<IEnumerable<DataSource>> ListAsync();
    Task<DataSource?> GetAsync(string id);
    Task<DataSource> CreateAsync(DataSource dataSource);
    Task<DataSource> UpdateAsync(string id, DataSource dataSource);
    Task<bool> DeleteAsync(string id);
    Task<bool> TestConnectionAsync(DataSource dataSource);
}
