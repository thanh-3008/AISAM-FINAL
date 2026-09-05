START TRANSACTION;
DROP TRIGGER execution_attribution_immutable ON execution_operations;
DROP FUNCTION aisam_execution_attribution_immutable();
DROP TRIGGER permission_revision_guard ON workspace_members;
DROP TRIGGER permission_revision_guard ON teams;
DROP TRIGGER permission_revision_guard ON team_members;
DROP TRIGGER permission_revision_guard ON team_brands;
DROP TRIGGER permission_revision_guard ON team_channel_access;
DROP TRIGGER permission_revision_guard ON brands;
DROP TRIGGER permission_revision_guard ON social_integrations;
DROP TRIGGER permission_revision_guard ON collaboration_tasks;
DROP TRIGGER permission_revision_guard ON temporary_access_grants;
DROP TRIGGER permission_revision_guard ON content_participations;
DROP TRIGGER permission_revision_guard ON workspaces;
DROP TRIGGER permission_revision_guard ON users;
DROP TRIGGER permission_revision_guard ON contents;
DROP FUNCTION aisam_permission_revision_guard();

ALTER TABLE workspaces DROP COLUMN permission_revision;

ALTER TABLE execution_operations DROP COLUMN execution_version;

DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260904112639_MutationPermissionRevision';

COMMIT;

