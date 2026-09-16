using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class PostCommitActionQueue : IPostCommitActionQueue
{
    private readonly List<Func<CancellationToken, Task>> _actions = [];

    public bool IsActive { get; private set; }

    public void Begin()
    {
        _actions.Clear();
        IsActive = true;
    }

    public bool TryEnqueue(Func<CancellationToken, Task> action)
    {
        if (!IsActive) return false;
        _actions.Add(action);
        return true;
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (!IsActive) return;
        var actions = _actions.ToArray();
        _actions.Clear();
        IsActive = false;
        foreach (var action in actions)
            await action(cancellationToken);
    }

    public void Discard()
    {
        _actions.Clear();
        IsActive = false;
    }
}
