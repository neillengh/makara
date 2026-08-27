namespace Makara.Infrastructure.Data;

public static class FreeSqlSetup
{
    public static IFreeSql Create(string connectionString)
    {
        return new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, connectionString)
            .UseAutoSyncStructure(true)
            .Build();
    }
}
