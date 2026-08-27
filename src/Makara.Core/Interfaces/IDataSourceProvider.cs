using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IDataSourceProvider
{
    Task<IEnumerable<Dictionary<string, object>>> ExtractAsync(DataSource dataSource, string? incrementalValue = null);
    Task<bool> TestConnectionAsync(DataSource dataSource);
    IEnumerable<string> GetTableNames(DataSource dataSource);
    IEnumerable<string> GetColumnNames(DataSource dataSource, string tableName);
}
