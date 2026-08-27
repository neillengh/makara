using Makara.Core.Enums;
using Makara.Core.Interfaces;

namespace Makara.Infrastructure.DataSourceProviders;

public class DataSourceProviderFactory
{
    private readonly Dictionary<DataSourceType, IDataSourceProvider> _providers;

    public DataSourceProviderFactory(
        SqlServerProvider sqlServer,
        MySqlProvider mySql,
        PostgreSqlProvider postgreSql,
        FileProvider fileProvider)
    {
        _providers = new()
        {
            [DataSourceType.SqlServer] = sqlServer,
            [DataSourceType.MySql] = mySql,
            [DataSourceType.PostgreSQL] = postgreSql,
            [DataSourceType.Csv] = fileProvider,
            [DataSourceType.Excel] = fileProvider
        };
    }

    public IDataSourceProvider? GetProvider(DataSourceType type) =>
        _providers.TryGetValue(type, out var provider) ? provider : null;
}
