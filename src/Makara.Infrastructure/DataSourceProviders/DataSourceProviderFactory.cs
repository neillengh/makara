using Makara.Core.Enums;
using Makara.Core.Interfaces;

namespace Makara.Infrastructure.DataSourceProviders;

public class DataSourceProviderFactory
{
    private readonly Dictionary<DataSourceType, IDataSourceProvider> _providers;

    public DataSourceProviderFactory(
        SqlServerProvider sqlServer,
        MySqlProvider mySql)
    {
        _providers = new()
        {
            [DataSourceType.SqlServer] = sqlServer,
            [DataSourceType.MySql] = mySql
        };
    }

    public IDataSourceProvider? GetProvider(DataSourceType type) =>
        _providers.TryGetValue(type, out var provider) ? provider : null;
}
