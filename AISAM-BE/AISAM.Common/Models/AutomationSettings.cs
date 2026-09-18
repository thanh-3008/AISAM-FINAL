namespace AISAM.Common.Models;

/// <summary>
/// Cấu hình AI Automation. Section: "AutomationSettings"
/// </summary>
public sealed class AutomationSettings
{
    /// <summary>
    /// Giới hạn thời gian timeout cho mỗi lượt sinh nội dung AI Automation (mặc định: 64 giây).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 64;
}
