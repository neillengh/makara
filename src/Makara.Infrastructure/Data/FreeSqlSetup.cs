using Makara.Core.Models;

namespace Makara.Infrastructure.Data;

public static class FreeSqlSetup
{
    public static IFreeSql Create(string connectionString)
    {
        var fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, connectionString)
            .UseAutoSyncStructure(true)
            .Build();

        return fsql;
    }
}
