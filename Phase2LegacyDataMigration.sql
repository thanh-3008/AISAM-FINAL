-- ==============================================================================
-- AISAM - PHASE 2 DATA MIGRATION: LEGACY FLAT RBAC -> DECOUPLED SCOPED RBAC
-- Target: PostgreSQL
-- Decisions: D-01 (Manager -> Member + TeamManager), D-02 (Content.team_id Backfill)
-- Trigger Safety: Bypasses 23514 by maintaining strict workspace scoping
-- Idempotent: Can be run multiple times safely
-- ==============================================================================

BEGIN;

-- ------------------------------------------------------------------------------
-- 1. TẠO DEFAULT TEAM CHO CÁC WORKSPACE CHƯA CÓ TEAM NÀO ĐANG HOẠT ĐỘNG
-- ------------------------------------------------------------------------------
INSERT INTO teams (id, workspace_id, name, description, status, is_deleted, created_at)
SELECT 
    gen_random_uuid(),
    w.id,
    'Default Team',
    'System generated team for legacy content and brand scoping',
    1, -- TeamStatusEnum.Active
    false,
    NOW()
FROM workspaces w
WHERE NOT EXISTS (
    SELECT 1 FROM teams t 
    WHERE t.workspace_id = w.id AND t.is_deleted = false AND t.status = 1
)
AND (
    EXISTS (SELECT 1 FROM contents c WHERE c.workspace_id = w.id AND c.team_id IS NULL)
    OR EXISTS (SELECT 1 FROM brands b WHERE b.workspace_id = w.id AND b.is_deleted = false)
);

-- ------------------------------------------------------------------------------
-- 2. ĐẢM BẢO MỌI BRAND CÓ CONTENT ĐỀU ĐƯỢC LIÊN KẾT VỚI ÍT NHẤT 1 TEAM (TEAM_BRANDS)
-- ------------------------------------------------------------------------------
INSERT INTO team_brands (id, team_id, brand_id, is_active)
SELECT 
    gen_random_uuid(),
    (
        SELECT t.id FROM teams t 
        WHERE t.workspace_id = b.workspace_id AND t.is_deleted = false AND t.status = 1 
        ORDER BY t.created_at ASC LIMIT 1
    ),
    b.id,
    true
FROM brands b
WHERE NOT EXISTS (
    SELECT 1 FROM team_brands tb 
    JOIN teams t ON t.id = tb.team_id 
    WHERE tb.brand_id = b.id AND tb.is_active = true AND t.workspace_id = b.workspace_id AND t.is_deleted = false
)
AND EXISTS (
    SELECT 1 FROM contents c WHERE c.brand_id = b.id AND c.team_id IS NULL
)
ON CONFLICT (team_id, brand_id) DO UPDATE SET is_active = true;

-- ------------------------------------------------------------------------------
-- 3. BACKFILL CONTENTS.TEAM_ID (D-02: 542 LEGACY CONTENTS THIẾU TEAM_ID)
-- ------------------------------------------------------------------------------
UPDATE contents c
SET team_id = COALESCE(
    -- 3a. Ưu tiên: Team đang active được nối với Brand của content đó
    (
        SELECT tb.team_id 
        FROM team_brands tb
        JOIN teams t ON t.id = tb.team_id
        WHERE tb.brand_id = c.brand_id 
          AND tb.is_active = true 
          AND t.workspace_id = c.workspace_id 
          AND t.is_deleted = false
          AND t.status = 1
        ORDER BY tb.id ASC
        LIMIT 1
    ),
    -- 3b. Fallback: Bất kỳ team active nào trong cùng workspace
    (
        SELECT t.id 
        FROM teams t 
        WHERE t.workspace_id = c.workspace_id 
          AND t.is_deleted = false 
          AND t.status = 1
        ORDER BY t.created_at ASC 
        LIMIT 1
    )
)
WHERE c.team_id IS NULL;

-- ------------------------------------------------------------------------------
-- 4. BẢO VỆ VÀ GÁN QUYỀN TEAM MANAGER CHO LEGACY MANAGERS (D-01)
-- Trước khi đổi role WorkspaceMember, đảm bảo mọi Manager cũ đều có bản ghi
-- TeamMember với Role = 1 (Manager) tại các Team thuộc Workspace đó.
-- ------------------------------------------------------------------------------
INSERT INTO team_members (id, team_id, user_id, role, permissions, joined_at, is_active)
SELECT 
    gen_random_uuid(),
    t.id,
    wm.user_id,
    1, -- TeamRoleEnum.Manager
    '[]'::jsonb,
    NOW(),
    true
FROM workspace_members wm
JOIN teams t ON t.workspace_id = wm.workspace_id AND t.is_deleted = false AND t.status = 1
WHERE wm.role = 2 AND wm.is_active = true -- 2 = Legacy Manager
ON CONFLICT (team_id, user_id) 
DO UPDATE SET role = 1, is_active = true;

-- ------------------------------------------------------------------------------
-- 5. LƯU VẾT VÀ REMAP WORKSPACE_MEMBERS.ROLE (D-01 & R-06)
-- ------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS _rbac_migration_wm_backup (
    workspace_member_id uuid PRIMARY KEY,
    workspace_id uuid NOT NULL,
    user_id uuid NOT NULL,
    old_role integer NOT NULL,
    new_role integer NOT NULL,
    migrated_at timestamp with time zone DEFAULT NOW()
);

-- Sao lưu trạng thái trước migrate
INSERT INTO _rbac_migration_wm_backup (workspace_member_id, workspace_id, user_id, old_role, new_role)
SELECT 
    wm.id, 
    wm.workspace_id, 
    wm.user_id, 
    wm.role,
    CASE 
        WHEN wm.role = 1 THEN 1 -- Owner -> Owner
        ELSE 3                  -- Manager(2), ContentCreator(3), Viewer(4) -> Member(3)
    END
FROM workspace_members wm
ON CONFLICT (workspace_member_id) DO NOTHING;

-- Cập nhật WorkspaceMember.Role sang giá trị mới
UPDATE workspace_members wm
SET role = b.new_role
FROM _rbac_migration_wm_backup b
WHERE wm.id = b.workspace_member_id AND wm.role <> b.new_role;

-- Cập nhật WorkspaceInvitation.Role
UPDATE workspace_invitations
SET role = CASE 
    WHEN role = 1 THEN 1
    ELSE 3 -- Member
END
WHERE role NOT IN (1, 3);

COMMIT;
