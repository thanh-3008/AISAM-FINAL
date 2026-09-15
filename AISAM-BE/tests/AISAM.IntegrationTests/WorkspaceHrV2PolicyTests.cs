using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Services.Access;

namespace AISAM.IntegrationTests;

public class WorkspaceHrV2PolicyTests
{
    [Theory]
    [InlineData(WorkspaceRoleV2.Owner,WorkspaceRoleV2.Member,true)]
    [InlineData(WorkspaceRoleV2.Owner,WorkspaceRoleV2.WorkspaceManager,true)]
    [InlineData(WorkspaceRoleV2.WorkspaceManager,WorkspaceRoleV2.Member,true)]
    [InlineData(WorkspaceRoleV2.WorkspaceManager,WorkspaceRoleV2.WorkspaceManager,false)]
    [InlineData(WorkspaceRoleV2.Member,WorkspaceRoleV2.Member,false)]
    [InlineData(WorkspaceRoleV2.Owner,WorkspaceRoleV2.Owner,false)]
    public void InviteHasNoPromotionPath(WorkspaceRoleV2 actorRole,WorkspaceRoleV2 requested,bool expected)
    {
        var actor=new WorkspaceMember{WorkspaceRoleV2=actorRole,IsActive=true};
        Assert.Equal(expected,WorkspaceHrV2Policy.CanInvite(actor,requested));
        actor.IsActive=false;
        Assert.False(WorkspaceHrV2Policy.CanInvite(actor,requested));
    }

    [Fact]
    public void ManagerCannotRemovePeerOrOwnerOrForeignMember()
    {
        var w=Guid.NewGuid();
        var actor=new WorkspaceMember{WorkspaceId=w,WorkspaceRoleV2=WorkspaceRoleV2.WorkspaceManager};
        var target=new WorkspaceMember{WorkspaceId=w,WorkspaceRoleV2=WorkspaceRoleV2.Member};
        Assert.True(WorkspaceHrV2Policy.CanRemove(actor,target));
        Assert.False(WorkspaceHrV2Policy.CanChangeRole(actor,target,WorkspaceRoleV2.WorkspaceManager));
        target.WorkspaceRoleV2=WorkspaceRoleV2.WorkspaceManager;
        Assert.False(WorkspaceHrV2Policy.CanRemove(actor,target));
        target.WorkspaceRoleV2=WorkspaceRoleV2.Owner;
        Assert.False(WorkspaceHrV2Policy.CanRemove(actor,target));
        target.WorkspaceRoleV2=WorkspaceRoleV2.Member;target.WorkspaceId=Guid.NewGuid();
        Assert.False(WorkspaceHrV2Policy.CanRemove(actor,target));
    }
}
