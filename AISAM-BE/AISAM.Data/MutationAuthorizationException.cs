namespace AISAM.Data;

public sealed class MutationAuthorizationException : Exception
{
    public MutationAuthorizationException() : base("Permission changed or expired before the write. Reload and retry.") { }
}
