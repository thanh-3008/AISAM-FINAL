-- READ ONLY. Run against a restored pre-CP-2 database, never infer ownership from ProfileId.
-- Produces counts and candidate IDs only. A candidate is evidence for review, not approval.
SELECT t.id AS team_id, COUNT(DISTINCT b.workspace_id) AS candidate_workspace_count,
       COUNT(*) FILTER (WHERE b.id IS NULL OR b.workspace_id IS NULL) AS missing_brand_links
FROM teams t LEFT JOIN team_brands tb ON tb.team_id = t.id
LEFT JOIN brands b ON b.id = tb.brand_id
GROUP BY t.id;

SELECT 'duplicate_team_member' AS issue, COUNT(*) AS groups
FROM (SELECT team_id, user_id FROM team_members GROUP BY team_id, user_id HAVING COUNT(*) > 1) d
UNION ALL
SELECT 'duplicate_team_brand', COUNT(*)
FROM (SELECT team_id, brand_id FROM team_brands GROUP BY team_id, brand_id HAVING COUNT(*) > 1) d
UNION ALL
SELECT 'content_brand_workspace_mismatch', COUNT(*) FROM contents c
LEFT JOIN brands b ON b.id = c.brand_id
WHERE b.id IS NULL OR c.workspace_id IS DISTINCT FROM b.workspace_id
UNION ALL
SELECT 'integration_brand_workspace_mismatch', COUNT(*) FROM social_integrations i
LEFT JOIN brands b ON b.id = i.brand_id
WHERE b.id IS NULL OR i.workspace_id IS DISTINCT FROM b.workspace_id;
