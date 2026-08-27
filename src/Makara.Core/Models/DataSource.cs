using Makara.Core.Enums;

namespace Makara.Core.Models;

public class DataSource
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public DataSourceType Type { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string? Query { get; set; }
    public string? IncrementalField { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
