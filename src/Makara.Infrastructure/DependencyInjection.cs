using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure.Data;
using Makara.Infrastructure.DataSourceProviders;
using Makara.Infrastructure.DataProcessing;
using Makara.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Makara.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMakaraInfrastructure(this IServiceCollection services, string connectionString)
    {
        var fsql = FreeSqlSetup.Create(connectionString);
        services.AddSingleton(fsql);

        // ===== 基础 3 表（原有）=====
        services.AddScoped<IRepository<DataSource>, FreeSqlRepository<DataSource>>();
        services.AddScoped<IRepository<Workflow>, FreeSqlRepository<Workflow>>();
        services.AddScoped<IRepository<WorkflowRun>, FreeSqlRepository<WorkflowRun>>();

        // ===== 新 6 表（扩表）=====
        services.AddScoped<IRepository<User>, FreeSqlRepository<User>>();
        services.AddScoped<IRepository<DatasetInfo>, FreeSqlRepository<DatasetInfo>>();
        services.AddScoped<IRepository<DatasetSample>, FreeSqlRepository<DatasetSample>>();
        services.AddScoped<IRepository<FieldMappingConfig>, FreeSqlRepository<FieldMappingConfig>>();
        services.AddScoped<IRepository<SystemSetting>, FreeSqlRepository<SystemSetting>>();
        services.AddScoped<IRepository<AuditLog>, FreeSqlRepository<AuditLog>>();

        services.AddScoped<SqlServerProvider>();
        services.AddScoped<MySqlProvider>();
        services.AddScoped<PostgreSqlProvider>();
        services.AddScoped<FileProvider>();
        services.AddScoped<DataSourceProviderFactory>();

        services.AddScoped<DataCleaner>();
        services.AddScoped<IDatasetBuilder, DatasetBuilder>();

        return services;
    }
}
