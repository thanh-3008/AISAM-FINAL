using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260911120000_BackfillTeamChannelAccessCanView")]
public partial class BackfillTeamChannelAccessCanView : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO team_channel_access (id, team_brand_id, integration_id, can_view, can_publish, can_manage)
            SELECT
                gen_random_uuid(), tb.id, si.id, true, false, false
            FROM team_brands tb
            JOIN social_integrations si ON si.brand_id = tb.brand_id
                AND si.workspace_id = (SELECT workspace_id FROM teams WHERE id = tb.team_id)
                AND si.is_deleted = false
            WHERE tb.is_active = true
            AND NOT EXISTS (
                SELECT 1 FROM team_channel_access tca
                WHERE tca.team_brand_id = tb.id AND tca.integration_id = si.id
            );
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM team_channel_access
            WHERE can_view = true AND can_publish = false AND can_manage = false
            AND NOT EXISTS (
                SELECT 1 FROM audit_logs al
                WHERE al.target_table = 'team_channel_access'
                AND al.target_id = team_channel_access.id
            );
        ");
    }
}
