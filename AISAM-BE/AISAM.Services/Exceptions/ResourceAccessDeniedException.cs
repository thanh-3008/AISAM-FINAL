namespace AISAM.Services.Exceptions;

public sealed class ResourceAccessDeniedException : Exception
{
    public ResourceAccessDeniedException() : base("The current permission does not allow this action.") { }
}
