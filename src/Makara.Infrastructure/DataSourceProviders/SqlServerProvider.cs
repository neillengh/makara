using System.Data;
using Makara.Core.Models;
using Microsoft.Data.SqlClient;

namespace Makara.Infrastructure.DataSourceProviders;

public class SqlServerProvider : RelationalDbProviderBase
{
    protected override IDbConnection CreateConnection(DataSource dataSource) =>
        new SqlConnection(dataSource.ConnectionString);

    public override IEnumerable<string> GetTableNames(DataSource dataSource)
    {
        using var conn = new SqlConnection(dataSource.ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            yield return reader.GetString(0);
    }

    public override IEnumerable<string> GetColumnNames(DataSource dataSource, string tableName)
    {
        using var conn = new SqlConnection(dataSource.ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @tableName
            ORDER BY ORDINAL_POSITION";
        cmd.Parameters.Add(new SqlParameter("@tableName", tableName));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            yield return reader.GetString(0);
    }
}
