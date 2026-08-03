using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    [DbContext(typeof(AisamContext))]
    [Migration("20260802121500_EnsureCampaignDeploymentMessageColumn")]
    public partial class EnsureCampaignDeploymentMessageColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE ad_campaigns ADD COLUMN IF NOT EXISTS deployment_message character varying(2000);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE ad_campaigns DROP COLUMN IF EXISTS deployment_message;");
        }
    }
}
