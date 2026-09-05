using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.IntegrationTests;

public sealed class ConversationAuthorizationSecurityTests
{
    [Fact]
    public async Task TeamA_ManagerCannotListReadDeleteOrReuseTeamBConversation()
    {
        await using var fixture = await ConversationFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Manager.Id,
            write: false);
        var service = fixture.Service();

        var list = await service.GetPagedByWorkspaceAsync(fixture.Security.Workspace.Id, new PaginationRequest());
        var detail = await service.GetByIdInWorkspaceAsync(fixture.TeamBConversation.Id, fixture.Security.Workspace.Id);
        var delete = await service.SoftDeleteInWorkspaceAsync(fixture.TeamBConversation.Id, fixture.Security.Workspace.Id);
        var reusable = await fixture.Repository().GetActiveByWorkspaceIdAsync(
            fixture.Security.Workspace.Id,
            fixture.TeamBConversation.BrandId,
            fixture.TeamBConversation.ProductId,
            fixture.TeamBConversation.AdType);

        Assert.DoesNotContain(list.Data!.Data, item => item.Id == fixture.TeamBConversation.Id);
        Assert.Equal((int)HttpStatusCode.NotFound, detail.StatusCode);
        Assert.Equal((int)HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Null(reusable);
        Assert.False(await fixture.Security.Db.Conversations
            .IgnoreQueryFilters()
            .Where(conversation => conversation.Id == fixture.TeamBConversation.Id)
            .Select(conversation => conversation.IsDeleted)
            .SingleAsync());
    }

    [Fact]
    public async Task ViewerListOmitsOutOfScopeAndInvalidProductConversationsAndSuppressesLastMessage()
    {
        await using var fixture = await ConversationFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Viewer.Id,
            write: false);
        var service = fixture.Service();

        var result = await service.GetPagedByWorkspaceAsync(fixture.Security.Workspace.Id, new PaginationRequest());

        var visible = Assert.Single(result.Data!.Data);
        Assert.Equal(fixture.VisibleConversation.Id, visible.Id);
        Assert.Null(visible.LastMessage);
        Assert.NotNull(visible.LastMessageAt);
        Assert.Equal(1, visible.MessageCount);
        Assert.DoesNotContain(result.Data.Data, item => item.Id == fixture.TeamBConversation.Id);
        Assert.DoesNotContain(result.Data.Data, item => item.Id == fixture.NullBrandConversation.Id);
        Assert.DoesNotContain(result.Data.Data, item => item.Id == fixture.MismatchedProductConversation.Id);
    }

    [Fact]
    public async Task ViewerCannotReadConversationBodiesEvenWhenBrandIsVisible()
    {
        await using var fixture = await ConversationFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Viewer.Id,
            write: false);

        var result = await fixture.Service().GetByIdInWorkspaceAsync(
            fixture.VisibleConversation.Id,
            fixture.Security.Workspace.Id);

        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Equal("RESOURCE_ACCESS_DENIED", result.Error?.ErrorCode);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ManagerCannotSeeLegacyNullBrandConversation()
    {
        await using var fixture = await ConversationFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Manager.Id,
            write: false);
        var service = fixture.Service();

        var list = await service.GetPagedByWorkspaceAsync(fixture.Security.Workspace.Id, new PaginationRequest());
        var detail = await service.GetByIdInWorkspaceAsync(
            fixture.NullBrandConversation.Id,
            fixture.Security.Workspace.Id);

        Assert.DoesNotContain(list.Data!.Data, item => item.Id == fixture.NullBrandConversation.Id);
        Assert.Equal((int)HttpStatusCode.NotFound, detail.StatusCode);
    }

    [Fact]
    public async Task CreatorCanReadLinkedPrimaryCreatorConversationButCannotDeleteAnotherCreatorsConversation()
    {
        await using var fixture = await ConversationFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Creator.Id,
            write: false);
        var service = fixture.Service();

        var detail = await service.GetByIdInWorkspaceAsync(
            fixture.LinkedConversation.Id,
            fixture.Security.Workspace.Id);
        var delete = await service.SoftDeleteInWorkspaceAsync(
            fixture.LinkedConversation.Id,
            fixture.Security.Workspace.Id);

        Assert.True(detail.Success);
        Assert.Equal(fixture.LinkedConversation.Id, detail.Data!.Id);
        Assert.Equal((int)HttpStatusCode.NotFound, delete.StatusCode);
        Assert.False(await fixture.Security.Db.Conversations
            .IgnoreQueryFilters()
            .Where(conversation => conversation.Id == fixture.LinkedConversation.Id)
            .Select(conversation => conversation.IsDeleted)
            .SingleAsync());
    }

    private sealed class ConversationFixture : IAsyncDisposable
    {
        public PermissionSecurityTests.Fixture Security { get; private init; } = null!;
        public Conversation VisibleConversation { get; private init; } = null!;
        public Conversation TeamBConversation { get; private init; } = null!;
        public Conversation NullBrandConversation { get; private init; } = null!;
        public Conversation MismatchedProductConversation { get; private init; } = null!;
        public Conversation LinkedConversation { get; private init; } = null!;

        public ConversationRepository Repository() => new(Security.Db);
        public ConversationService Service() => new(Repository(), Security.Db.AccessScope);

        public static async Task<ConversationFixture> CreateAsync()
        {
            var security = await PermissionSecurityTests.Fixture.CreateAsync();
            var otherCreatorLink = await security.Db.TeamMembers
                .SingleAsync(member => member.TeamId == security.Team.Id && member.UserId == security.OtherCreator.Id);
            otherCreatorLink.IsActive = false;
            var teamB = new Team
            {
                WorkspaceId = security.Workspace.Id,
                Name = "Team B",
                Status = TeamStatusEnum.Active
            };
            teamB.TeamMembers.Add(new TeamMember
            {
                TeamId = teamB.Id,
                UserId = security.OtherCreator.Id,
                Role = nameof(WorkspaceMemberRoleEnum.ContentCreator)
            });
            var brandB = new Brand
            {
                WorkspaceId = security.Workspace.Id,
                ProfileId = security.Profile.Id,
                Name = "Team B brand"
            };
            teamB.TeamBrands.Add(new TeamBrand
            {
                TeamId = teamB.Id,
                BrandId = brandB.Id,
                ChannelAccessMode = ChannelAccessMode.Specific
            });
            var productB = new Product { BrandId = brandB.Id, Name = "Team B product" };
            var otherProfile = new Profile
            {
                WorkspaceId = security.Workspace.Id,
                UserId = security.OtherCreator.Id,
                Name = "Other creator"
            };
            security.Db.AddRange(teamB, brandB, productB, otherProfile);

            var fixture = new ConversationFixture
            {
                Security = security,
                VisibleConversation = NewConversation(security.Workspace.Id, security.Profile.Id, security.Brand.Id, null, "Visible"),
                TeamBConversation = NewConversation(security.Workspace.Id, otherProfile.Id, brandB.Id, productB.Id, "Team B"),
                NullBrandConversation = NewConversation(security.Workspace.Id, otherProfile.Id, null, null, "Legacy null Brand"),
                MismatchedProductConversation = NewConversation(security.Workspace.Id, security.Profile.Id, security.Brand.Id, productB.Id, "Mismatched product"),
                LinkedConversation = NewConversation(security.Workspace.Id, otherProfile.Id, brandB.Id, null, "Linked content")
            };
            fixture.VisibleConversation.ChatMessages.Add(NewMessage(fixture.VisibleConversation.Id, "private viewer body"));
            fixture.TeamBConversation.ChatMessages.Add(NewMessage(fixture.TeamBConversation.Id, "team B body"));
            fixture.LinkedConversation.ChatMessages.Add(new ChatMessage
            {
                ConversationId = fixture.LinkedConversation.Id,
                SenderType = ChatSenderType.AI,
                Message = "linked body",
                ContentId = security.OwnContent.Id
            });
            security.Db.Conversations.AddRange(
                fixture.VisibleConversation,
                fixture.TeamBConversation,
                fixture.NullBrandConversation,
                fixture.MismatchedProductConversation,
                fixture.LinkedConversation);
            security.Db.SaveChanges();
            return fixture;
        }

        private static Conversation NewConversation(
            Guid workspaceId,
            Guid profileId,
            Guid? brandId,
            Guid? productId,
            string title) => new()
            {
                WorkspaceId = workspaceId,
                ProfileId = profileId,
                BrandId = brandId,
                ProductId = productId,
                Title = title,
                AdType = AdTypeEnum.TextOnly,
                IsActive = true
            };

        private static ChatMessage NewMessage(Guid conversationId, string message) => new()
        {
            ConversationId = conversationId,
            SenderType = ChatSenderType.User,
            Message = message
        };

        public ValueTask DisposeAsync() => Security.DisposeAsync();
    }
}
