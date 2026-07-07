namespace AISAM.Data.Enumeration;

public enum AutomationPlanStatusEnum
{
    Uploaded = 0,
    Validating = 1,
    AwaitingConfirmation = 2,
    Generating = 3,
    AwaitingApproval = 4,
    Scheduling = 5,
    Completed = 6,
    PartiallyFailed = 7,
    Failed = 8,
    Cancelled = 9
}

public enum AutomationItemStatusEnum
{
    Pending = 0,
    NeedsAttention = 1,
    GeneratingText = 2,
    GeneratingMedia = 3,
    QualityCheck = 4,
    AwaitingApproval = 5,
    Approved = 6,
    Scheduled = 7,
    Published = 8,
    Rejected = 9,
    GenerationFailed = 10,
    PublishFailed = 11
}

public enum AutomationContentTypeEnum
{
    Text = 0,
    Image = 1,
    Video = 2,
    Auto = 3
}
