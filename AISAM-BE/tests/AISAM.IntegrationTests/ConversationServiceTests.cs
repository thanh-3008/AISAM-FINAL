using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class ConversationServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_ForAnotherProfile()
    {
        var conversation = CreateConversation(Guid.NewGuid());
        var service = new ConversationService(new FakeConversationRepository(conversation));

        var result = await service.GetByIdAsync(conversation.Id, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotDeleteAnotherProfilesConversation()
    {
        var conversation = CreateConversation(Guid.NewGuid());
        var repository = new FakeConversationRepository(conversation);
        var service = new ConversationService(repository);

        var result = await service.SoftDeleteAsync(conversation.Id, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(repository.UpdateCalled);
        Assert.False(conversation.IsDeleted);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsOnlyActiveProfilesConversations()
    {
        var profileId = Guid.NewGuid();
        var owned = CreateConversation(profileId);
        var other = CreateConversation(Guid.NewGuid());
        var repository = new FakeConversationRepository(owned, other);
        var service = new ConversationService(repository);

        var result = await service.GetPagedAsync(profileId, new PaginationRequest());

        Assert.True(result.Success);
        Assert.Single(result.Data!.Data);
        Assert.Equal(owned.Id, result.Data.Data.Single().Id);
        Assert.Equal(profileId, repository.LastPagedProfileId);
    }

    [Fact]
    public async Task GetByIdAsync_RecoversContentId_ForLegacyVideoMessage()
    {
        var profileId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        var generationId = Guid.NewGuid();
        var conversation = CreateConversation(profileId);
        conversation.ChatMessages.Add(new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderType = ChatSenderType.AI,
            Message = "Video is processing [VIDEO_JOB: legacy-job]"
        });
        var repository = new FakeConversationRepository(conversation);
        repository.GenerationsByVideoJobId["legacy-job"] = new AiGeneration
        {
            Id = generationId,
            ContentId = contentId,
            VideoJobId = "legacy-job"
        };
        var service = new ConversationService(repository);

        var result = await service.GetByIdAsync(conversation.Id, profileId);

        var restoredMessage = Assert.Single(result.Data!.Messages);
        Assert.Equal(contentId, restoredMessage.ContentId);
        Assert.Equal(generationId, restoredMessage.AiGenerationId);
    }

    private static Conversation CreateConversation(Guid profileId)
    {
        return new Conversation
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Title = "Conversation",
            AdType = AdTypeEnum.TextOnly
        };
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Dictionary<Guid, Conversation> _conversations;

        public FakeConversationRepository(params Conversation[] conversations)
        {
            _conversations = conversations.ToDictionary(conversation => conversation.Id);
        }

        public Guid LastPagedProfileId { get; private set; }
        public bool UpdateCalled { get; private set; }
        public Dictionary<string, AiGeneration> GenerationsByVideoJobId { get; } = new(StringComparer.Ordinal);

        public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_conversations.GetValueOrDefault(id));
        }

        public Task<PagedResult<Conversation>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            LastPagedProfileId = profileId;
            var data = _conversations.Values.Where(conversation => conversation.ProfileId == profileId && !conversation.IsDeleted).ToList();
            return Task.FromResult(new PagedResult<Conversation>
            {
                Data = data,
                TotalCount = data.Count,
                Page = 1,
                PageSize = 10
            });
        }

        public Task<Conversation?> GetActiveAsync(Guid profileId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            UpdateCalled = true;
            _conversations[conversation.Id] = conversation;
            return Task.CompletedTask;
        }

        public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IReadOnlyDictionary<string, AiGeneration>> GetGenerationsByVideoJobIdsAsync(
            Guid workspaceId,
            IEnumerable<string> videoJobIds,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyDictionary<string, AiGeneration> result = videoJobIds
                .Where(GenerationsByVideoJobId.ContainsKey)
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(jobId => jobId, jobId => GenerationsByVideoJobId[jobId], StringComparer.Ordinal);
            return Task.FromResult(result);
        }
    }
}
