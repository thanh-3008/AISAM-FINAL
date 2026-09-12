namespace AISAM.Repositories;
public sealed class ResourceMutationDeniedException : UnauthorizedAccessException
{
    public ResourceMutationDeniedException() : base("Resource mutation is outside the authorized scope.") { }
}
