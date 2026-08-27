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

        // FreeSql 3.5 不支持把 List<T> 强类型集合直接映射为 JSON 列，
        // 因此将 Nodes/Edges/Logs 排除出 DB 映射，改由服务层手动序列化到 *Json 字符串列。
        fsql.CodeFirst
            .ConfigEntity<Workflow>(c =>
            {
                c.Property(w => w.Nodes).IsIgnore(true);
                c.Property(w => w.Edges).IsIgnore(true);
            })
            .ConfigEntity<WorkflowRun>(c =>
            {
                c.Property(r => r.Logs).IsIgnore(true);
            });

        return fsql;
    }
}
