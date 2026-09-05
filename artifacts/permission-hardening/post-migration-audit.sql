-- READ ONLY. Run on a restored database after the CP-2 migrations.
-- For a PRE-CP-2 database, use ../permission-final-pass/legacy-mapping-audit.sql.
-- Counts are diagnostics, not permission grants or authority to repair ownership.
BEGIN READ ONLY;
SELECT 'team_without_workspace' AS issue, COUNT(*) AS affected FROM teams t
LEFT JOIN workspaces w ON w.id = t.workspace_id WHERE w.id IS NULL
UNION ALL SELECT 'duplicate_team_member', COUNT(*) FROM
  (SELECT team_id,user_id FROM team_members GROUP BY team_id,user_id HAVING COUNT(*) > 1) d
UNION ALL SELECT 'duplicate_team_brand', COUNT(*) FROM
  (SELECT team_id,brand_id FROM team_brands GROUP BY team_id,brand_id HAVING COUNT(*) > 1) d
UNION ALL SELECT 'team_brand_workspace_mismatch', COUNT(*) FROM team_brands b
  LEFT JOIN teams t ON t.id=b.team_id LEFT JOIN brands brand ON brand.id=b.brand_id
  WHERE t.id IS NULL OR brand.id IS NULL OR t.workspace_id IS DISTINCT FROM brand.workspace_id
UNION ALL SELECT 'invalid_channel_brand_link', COUNT(*) FROM team_channel_access c
  LEFT JOIN team_brands b ON b.id=c.team_brand_id LEFT JOIN teams t ON t.id=b.team_id
  LEFT JOIN social_integrations i ON i.id=c.integration_id
  WHERE b.id IS NULL OR i.id IS NULL OR b.brand_id IS DISTINCT FROM i.brand_id
    OR t.workspace_id IS DISTINCT FROM i.workspace_id
UNION ALL SELECT 'campaign_without_channel', COUNT(*) FROM ad_campaigns WHERE integration_id IS NULL
UNION ALL SELECT 'orphan_or_foreign_content_brand', COUNT(*) FROM contents c
  LEFT JOIN brands b ON b.id=c.brand_id WHERE b.id IS NULL OR c.workspace_id IS DISTINCT FROM b.workspace_id
UNION ALL SELECT 'missing_primary_creator', COUNT(*) FROM contents WHERE primary_creator_id IS NULL
UNION ALL SELECT 'orphan_or_foreign_task', COUNT(*) FROM collaboration_tasks t
  LEFT JOIN contents c ON c.id=t.content_id LEFT JOIN teams team ON team.id=t.team_id
  WHERE c.id IS NULL OR team.id IS NULL OR t.workspace_id IS DISTINCT FROM c.workspace_id
    OR t.workspace_id IS DISTINCT FROM team.workspace_id
UNION ALL SELECT 'orphan_or_mismatched_grant', COUNT(*) FROM temporary_access_grants g
  LEFT JOIN collaboration_tasks t ON t.id=g.task_id
  WHERE t.id IS NULL OR g.workspace_id IS DISTINCT FROM t.workspace_id OR g.user_id IS DISTINCT FROM t.assignee_id
UNION ALL SELECT 'invalid_grant_dates', COUNT(*) FROM temporary_access_grants WHERE expires_at <= granted_at
UNION ALL SELECT 'execution_missing_event_team', COUNT(*) FROM execution_operations WHERE team_id IS NULL
UNION ALL SELECT 'execution_missing_enqueue_proof', COUNT(*) FROM execution_operations WHERE enqueue_authorized_at IS NULL
UNION ALL SELECT 'execution_unapproved_version', COUNT(*) FROM execution_operations WHERE execution_version <= 0 OR policy_version <= 0
UNION ALL SELECT 'schedule_without_execution_context', COUNT(*) FROM content_calendar c WHERE NOT EXISTS
  (SELECT 1 FROM execution_operations e WHERE e.resource_type='ContentCalendar' AND e.reference_id=c.id)
UNION ALL SELECT 'ai_job_without_original_actor', COUNT(*) FROM ai_generations g WHERE NOT EXISTS
  (SELECT 1 FROM execution_operations e WHERE e.resource_type='AiGeneration' AND e.reference_id=g.id);

-- Confirm trigger installation without printing any connection/configuration data.
SELECT event_object_table, trigger_name, action_timing, event_manipulation
FROM information_schema.triggers WHERE trigger_name IN ('permission_revision_guard','execution_attribution_immutable')
ORDER BY event_object_table, event_manipulation;
COMMIT;
