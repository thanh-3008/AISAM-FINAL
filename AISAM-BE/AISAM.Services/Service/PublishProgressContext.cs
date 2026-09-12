using AISAM.Common.Models;
namespace AISAM.Services.Service;

// One request/worker scope; never shared across requests.
public sealed class PublishProgressContext
{
    public Func<string,IReadOnlyList<PublishMediaResult>,CancellationToken,Task>? Report {get;set;}
}
