namespace AISAM.Services.IServices;

/// <summary>
/// Holds external side effects until the surrounding workspace mutation has
/// committed. This prevents sending links for database rows that later roll
/// back because of an optimistic-concurrency conflict.
/// </summary>
public interface IPostCommitActionQueue
{
    bool IsActive { get; }
    void Begin();
    bool TryEnqueue(Func<CancellationToken, Task> action);
    Task CompleteAsync(CancellationToken cancellationToken = default);
    void Discard();
}
