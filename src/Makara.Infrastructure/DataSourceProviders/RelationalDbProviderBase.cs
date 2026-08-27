using System.Data;
using System.Data.Common;
using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Infrastructure.DataSourceProviders;

public abstract class RelationalDbProviderBase : IDataSourceProvider
{
    protected abstract IDbConnection CreateConnection(DataSource dataSource);

    public async Task<bool> TestConnectionAsync(DataSource dataSource)
    {
        try
        {
            using var conn = CreateConnection(dataSource);
            await ((DbConnection)conn).OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ExtractAsync(
        DataSource dataSource, string? incrementalValue = null)
    {
        var query = BuildQuery(dataSource, incrementalValue);
        var results = new List<Dictionary<string, object>>();

        using var conn = CreateConnection(dataSource);
        await ((DbConnection)conn).OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = query;

        if (!string.IsNullOrEmpty(dataSource.IncrementalField) && incrementalValue != null)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = "@incrementalValue";
            param.Value = Convert.ChangeType(incrementalValue, typeof(string));
            cmd.Parameters.Add(param);
        }

        using var reader = await ((DbCommand)cmd).ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
            results.Add(row);
        }

        return results;
    }

    public abstract IEnumerable<string> GetTableNames(DataSource dataSource);

    public abstract IEnumerable<string> GetColumnNames(DataSource dataSource, string tableName);

    protected virtual string BuildQuery(DataSource dataSource, string? incrementalValue)
    {
        if (!string.IsNullOrEmpty(dataSource.Query))
        {
            if (!string.IsNullOrEmpty(dataSource.IncrementalField) && incrementalValue != null)
                return $"{dataSource.Query} AND {dataSource.IncrementalField} > @incrementalValue";
            return dataSource.Query;
        }
        return string.Empty;
    }
}
