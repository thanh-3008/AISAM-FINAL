-- Safe repair script for profiles created before AddProfileWorkspaceOwnership.
-- Run after the migration has added profiles.workspace_id and before promoting to staging.
-- The EF model uses lower-case PostgreSQL names: profiles.workspace_id.

BEGIN;

-- Required audit query from the task.
SELECT *
FROM profiles
WHERE workspace_id IS NULL;

CREATE TEMP TABLE aisam_orphan_profile_workspace_map
(
    profile_id uuid PRIMARY KEY,
    user_id uuid NOT NULL,
    workspace_id uuid NOT NULL
) ON COMMIT DROP;

INSERT INTO aisam_orphan_profile_workspace_map (profile_id, user_id, workspace_id)
SELECT
    p.id,
    p.user_id,
    md5('aisam-profile-workspace-ownership:' || p.id::text)::uuid AS workspace_id
FROM profiles p
WHERE p.workspace_id IS NULL;

DO $$
DECLARE
    conflict_count bigint;
BEGIN
    SELECT COUNT(*)
    INTO conflict_count
    FROM aisam_orphan_profile_workspace_map map
    JOIN profiles existing
      ON existing.workspace_id = map.workspace_id
     AND existing.id <> map.profile_id;

    IF conflict_count > 0 THEN
        RAISE EXCEPTION 'Cannot repair orphan profiles: % deterministic workspace ids are already assigned', conflict_count;
    END IF;
END $$;

INSERT INTO workspaces
    (id, name, workspace_type, status, member_limit, company_name, bio, avatar_url, created_at, updated_at)
SELECT
    map.workspace_id,
    LEFT(COALESCE(NULLIF(TRIM(p.name), ''), 'Personal') || ' Workspace', 255),
    1,
    1,
    1,
    p.company_name,
    p.bio,
    p.avatar_url,
    COALESCE(p.created_at, CURRENT_TIMESTAMP),
    CURRENT_TIMESTAMP
FROM aisam_orphan_profile_workspace_map map
JOIN profiles p ON p.id = map.profile_id
WHERE NOT EXISTS (
    SELECT 1
    FROM workspaces w
    WHERE w.id = map.workspace_id
);

INSERT INTO workspace_members
    (id, workspace_id, user_id, role, quota_mode, credit_limit, credit_used, credit_period_start, joined_at, is_active)
SELECT
    md5('aisam-profile-workspace-owner:' || map.profile_id::text)::uuid,
    map.workspace_id,
    map.user_id,
    1,
    1,
    NULL,
    0,
    NULL,
    CURRENT_TIMESTAMP,
    TRUE
FROM aisam_orphan_profile_workspace_map map
WHERE NOT EXISTS (
    SELECT 1
    FROM workspace_members wm
    WHERE wm.workspace_id = map.workspace_id
      AND wm.user_id = map.user_id
);

INSERT INTO credit_wallets
    (id, workspace_id, balance, created_at, updated_at)
SELECT
    md5('aisam-profile-workspace-wallet:' || map.workspace_id::text)::uuid,
    map.workspace_id,
    0,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM aisam_orphan_profile_workspace_map map
WHERE NOT EXISTS (
    SELECT 1
    FROM credit_wallets wallet
    WHERE wallet.workspace_id = map.workspace_id
);

UPDATE profiles p
SET workspace_id = map.workspace_id,
    updated_at = CURRENT_TIMESTAMP
FROM aisam_orphan_profile_workspace_map map
WHERE p.id = map.profile_id
  AND p.workspace_id IS NULL;

DO $$
DECLARE
    missing_count bigint;
BEGIN
    SELECT COUNT(*)
    INTO missing_count
    FROM profiles
    WHERE workspace_id IS NULL;

    IF missing_count > 0 THEN
        RAISE EXCEPTION 'Profile workspace repair left % orphan profiles unmapped', missing_count;
    END IF;
END $$;

SELECT
    COUNT(*) AS remaining_orphan_profiles
FROM profiles
WHERE workspace_id IS NULL;

COMMIT;
