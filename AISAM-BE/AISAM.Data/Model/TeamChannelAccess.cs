using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

[Table("team_channel_access")]
public class TeamChannelAccess
{
    [Key, Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Column("team_brand_id")]
    public Guid TeamBrandId { get; set; }
    [Column("integration_id")]
    public Guid IntegrationId { get; set; }
    [Column("can_view")] public bool CanView { get; set; }
    [Column("can_publish")] public bool CanPublish { get; set; }
    [Column("can_manage")] public bool CanManage { get; set; }
    public virtual TeamBrand TeamBrand { get; set; } = null!;
    public virtual SocialIntegration Integration { get; set; } = null!;
}
