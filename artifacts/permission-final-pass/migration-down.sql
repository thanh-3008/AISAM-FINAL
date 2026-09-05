START TRANSACTION;
DO $$ BEGIN
  IF EXISTS (SELECT 1 FROM execution_operations) OR EXISTS (
    SELECT 1 FROM audit_logs WHERE requested_by IS NOT NULL OR executed_by_system) THEN
    RAISE EXCEPTION 'Rollback requires a reviewed attribution archive; no changes applied.';
  END IF;
END $$;
ALTER TABLE collaboration_tasks DROP CONSTRAINT fk_collaboration_content_workspace;
ALTER TABLE content_participations DROP CONSTRAINT fk_participation_content_workspace;
ALTER TABLE temporary_access_grants DROP CONSTRAINT ck_temporary_grant_dates;

ALTER TABLE collaboration_tasks DROP CONSTRAINT "FK_collaboration_tasks_teams_team_id_workspace_id";

ALTER TABLE temporary_access_grants DROP CONSTRAINT "FK_temporary_access_grants_collaboration_tasks_task_id_workspa~";

DROP TABLE execution_operations;

DROP INDEX "IX_temporary_access_grants_task_id_workspace_id";

ALTER TABLE teams DROP CONSTRAINT "AK_teams_id_workspace_id";

DROP INDEX "IX_contents_id_workspace_id";

ALTER TABLE collaboration_tasks DROP CONSTRAINT "AK_collaboration_tasks_id_workspace_id";

DROP INDEX "IX_collaboration_tasks_team_id_workspace_id";

ALTER TABLE audit_logs DROP COLUMN affected_user_id;

ALTER TABLE audit_logs DROP COLUMN approved_by;

ALTER TABLE audit_logs DROP COLUMN executed_by_system;

ALTER TABLE audit_logs DROP COLUMN reference_id;

ALTER TABLE audit_logs DROP COLUMN requested_by;

ALTER TABLE audit_logs DROP COLUMN team_id;

CREATE INDEX "IX_temporary_access_grants_task_id" ON temporary_access_grants (task_id);

CREATE INDEX "IX_collaboration_tasks_team_id" ON collaboration_tasks (team_id);

DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity';

COMMIT;

