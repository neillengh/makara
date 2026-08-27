using System.Data;
using Makara.Core.Models;
using Npgsql;

namespace Makara.Infrastructure.DataSourceProviders;

public class PostgreSqlProvider : RelationalDbProviderBase
{
    protected override IDbConnection CreateConnection(DataSource dataSource) =>
        new NpgsqlConnection(dataSource.ConnectionString);

    public override IEnumerable<string> GetTableNames(DataSource dataSource)
    {
        using var conn = new NpgsqlConnection(dataSource.ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            AND TABLE_SCHEMA = current_schema()
            ORDER BY TABLE_NAME";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            yield return reader.GetString(0);
    }

    public override IEnumerable<string> GetColumnNames(DataSource dataSource, string tableName)
    {
        using var conn = new NpgsqlConnection(dataSource.ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = current_schema()
            AND TABLE_NAME = @tableName
            ORDER BY ORDINAL_POSITION";
        cmd.Parameters.AddWithValue("@tableName", tableName);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            yield return reader.GetString(0);
    }
}
