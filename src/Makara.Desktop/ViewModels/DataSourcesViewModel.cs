using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Core.Enums;
using Makara.Core.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

public partial class DataSourcesViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty]
    private ObservableCollection<DataSource> _dataSources = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TestConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _newName = string.Empty;

    [ObservableProperty]
    private DataSourceType _newType = DataSourceType.SqlServer;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TestConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _newConnectionString = string.Empty;

    [ObservableProperty]
    private string _newQuery = string.Empty;

    [ObservableProperty]
    private DataSource? _selectedDataSource;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public IEnumerable<DataSourceType> DataSourceTypes => Enum.GetValues<DataSourceType>();

    [ObservableProperty]
    private string _connectionStringHint = string.Empty;

    [ObservableProperty]
    private string _queryHint = string.Empty;

    public DataSourcesViewModel(ApiClient api)
    {
        _api = api;
        UpdateHints(DataSourceType.SqlServer);
    }

    partial void OnNewTypeChanged(DataSourceType value) => UpdateHints(value);

    private void UpdateHints(DataSourceType type)
    {
        ConnectionStringHint = type switch
        {
            DataSourceType.SqlServer => "Server=localhost,1433;Database=mydb;User Id=sa;Password=xxx;TrustServerCertificate=True",
            DataSourceType.MySql => "Server=localhost;Port=3306;Database=mydb;User Id=root;Password=xxx",
            DataSourceType.PostgreSQL => "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=xxx",
            DataSourceType.Csv => "文件完整路径，如 C:\\data\\input.csv",
            DataSourceType.Excel => "文件完整路径，如 C:\\data\\input.xlsx",
            DataSourceType.Json => "文件完整路径，如 C:\\data\\input.json",
            _ => string.Empty
        };

        QueryHint = type switch
        {
            DataSourceType.SqlServer or DataSourceType.MySql or DataSourceType.PostgreSQL
                => "SQL 查询语句，如 SELECT * FROM users WHERE status = 'active'",
            DataSourceType.Csv or DataSourceType.Excel
                => "工作表名称（可选，留空读取第一个）",
            _ => string.Empty
        };
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        try
        {
            var list = await _api.GetDataSourcesAsync();
            DataSources.Clear();
            if (list != null)
                foreach (var ds in list)
                    DataSources.Add(ds);
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanTestConnection))]
    private async Task TestConnectionAsync()
    {
        var ds = new DataSource
        {
            Name = NewName,
            Type = NewType,
            ConnectionString = NewConnectionString,
            Query = NewQuery
        };

        IsLoading = true;
        StatusMessage = "正在测试连接...";
        try
        {
            var ok = await _api.TestConnectionAsync(ds);
            StatusMessage = ok ? "连接成功" : "连接失败";
        }
        catch (Exception ex)
        {
            StatusMessage = $"连接错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {
        var ds = new DataSource
        {
            Name = NewName,
            Type = NewType,
            ConnectionString = NewConnectionString,
            Query = string.IsNullOrWhiteSpace(NewQuery) ? null : NewQuery
        };

        IsLoading = true;
        StatusMessage = "正在创建...";
        try
        {
            var created = await _api.CreateDataSourceAsync(ds);
            if (created != null)
                DataSources.Add(created);
            StatusMessage = "创建成功";
            NewName = string.Empty;
            NewConnectionString = string.Empty;
            NewQuery = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"创建失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(DataSource ds)
    {
        try
        {
            await _api.DeleteDataSourceAsync(ds.Id);
            DataSources.Remove(ds);
            StatusMessage = "已删除";
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }

    private bool CanTestConnection() => !string.IsNullOrWhiteSpace(NewName) && !string.IsNullOrWhiteSpace(NewConnectionString);
    private bool CanCreate() => !string.IsNullOrWhiteSpace(NewName) && !string.IsNullOrWhiteSpace(NewConnectionString);
}
