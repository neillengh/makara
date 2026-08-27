using Makara.Core.Interfaces;
using Makara.Core.Models;
using MiniExcelLibs;

namespace Makara.Infrastructure.DataSourceProviders;

public class FileProvider : IDataSourceProvider
{
    public Task<bool> TestConnectionAsync(DataSource dataSource)
    {
        var exists = File.Exists(dataSource.ConnectionString);
        return Task.FromResult(exists);
    }

    public Task<IEnumerable<Dictionary<string, object>>> ExtractAsync(
        DataSource dataSource, string? incrementalValue = null)
    {
        var filePath = dataSource.ConnectionString;
        var sheetName = string.IsNullOrEmpty(dataSource.Query) ? null : dataSource.Query;

        var rows = sheetName is null
            ? MiniExcel.Query(filePath)
            : MiniExcel.Query(filePath, sheetName: sheetName);

        var results = new List<Dictionary<string, object>>();
        foreach (var row in rows)
        {
            var dict = new Dictionary<string, object>();
            if (row is IDictionary<string, object?> rowDict)
            {
                foreach (var kvp in rowDict)
                    dict[kvp.Key] = kvp.Value ?? string.Empty;
            }
            results.Add(dict);
        }

        return Task.FromResult<IEnumerable<Dictionary<string, object>>>(results);
    }

    public IEnumerable<string> GetTableNames(DataSource dataSource)
    {
        var filePath = dataSource.ConnectionString;
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        if (ext == ".csv")
            return [Path.GetFileNameWithoutExtension(filePath)];

        return MiniExcel.GetSheetNames(filePath);
    }

    public IEnumerable<string> GetColumnNames(DataSource dataSource, string tableName)
    {
        var filePath = dataSource.ConnectionString;

        var firstRow = string.IsNullOrEmpty(tableName)
            ? MiniExcel.Query(filePath).FirstOrDefault()
            : MiniExcel.Query(filePath, sheetName: tableName).FirstOrDefault();

        if (firstRow is IDictionary<string, object?> dict)
            return dict.Keys;

        return [];
    }
}
