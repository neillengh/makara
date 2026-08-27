using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.Services;

public class DataSourceService : IDataSourceService
{
    private readonly IRepository<DataSource> _repo;

    public DataSourceService(IRepository<DataSource> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<DataSource>> ListAsync() =>
        await _repo.GetAllAsync();

    public async Task<DataSource?> GetAsync(string id) =>
        await _repo.GetByIdAsync(id);

    public async Task<DataSource> CreateAsync(DataSource dataSource) =>
        await _repo.InsertAsync(dataSource);

    public async Task<DataSource> UpdateAsync(string id, DataSource dataSource)
    {
        dataSource.Id = id;
        dataSource.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(dataSource);
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task<bool> TestConnectionAsync(DataSource dataSource)
    {
        // TODO: 实现实际连接测试
        await Task.Delay(100);
        return true;
    }
}
