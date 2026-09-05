using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

public enum ChannelAccessMode { All, Specific }

[Table("team_channel_access")]
public sealed class TeamChannelAccess
{
    [Key, Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
    [Column("team_brand_id")] public Guid TeamBrandId { get; set; }
    [Column("integration_id")] public Guid IntegrationId { get; set; }
    public TeamBrand TeamBrand { get; set; } = null!;
    public SocialIntegration Integration { get; set; } = null!;
}
