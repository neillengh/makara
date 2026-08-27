using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IProgressHub
{
    void Publish(WorkflowEvent evt);
    IAsyncEnumerable<WorkflowEvent> Subscribe(string runId, CancellationToken ct = default);
    void Complete(string runId);
}
