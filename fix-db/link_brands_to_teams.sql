-- ==============================================================================
-- AISAM: Link Existing Brands to Teams & Reconcile Content Ownership
-- ==============================================================================

BEGIN;

-- 1. Đảm bảo mọi Workspace đều có ít nhất 1 Team đang hoạt động
-- Lưu ý: TeamStatusEnum: Active = 0, Inactive = 1, Archived = 2
UPDATE teams SET status = 0 WHERE status = 1 AND is_deleted = FALSE;

INSERT INTO teams (id, workspace_id, name, created_at, updated_at, is_deleted, status)
SELECT 
    gen_random_uuid(),
    w.id,
    w.name || ' Team',
    NOW(),
    NOW(),
    FALSE,
    0 -- 0 = Active
FROM workspaces w
WHERE NOT EXISTS (
    SELECT 1 FROM teams t 
    WHERE t.workspace_id = w.id AND t.is_deleted = FALSE
);

-- 2. Đảm bảo các thành viên Workspace (đặc biệt là Owner) được liên kết vào Team của Workspace
INSERT INTO team_members (id, team_id, user_id, role, joined_at, is_active)
SELECT 
    gen_random_uuid(),
    t.id,
    wm.user_id,
    CASE 
        WHEN wm.role = 1 THEN 'Owner'
        WHEN wm.role = 2 THEN 'Manager'
        WHEN wm.role = 3 THEN 'ContentCreator'
        ELSE 'Viewer'
    END,
    NOW(),
    TRUE
FROM workspace_members wm
JOIN teams t ON t.workspace_id = wm.workspace_id AND t.is_deleted = FALSE
WHERE wm.is_active = TRUE
AND NOT EXISTS (
    SELECT 1 FROM team_members tm 
    WHERE tm.team_id = t.id AND tm.user_id = wm.user_id
);

-- 3. Liên kết toàn bộ Brands hiện có vào Team của Workspace trong bảng team_brands
INSERT INTO team_brands (id, team_id, brand_id, assigned_at, is_active, channel_access_mode)
SELECT 
    gen_random_uuid(),
    t.id,
    b.id,
    NOW(),
    TRUE,
    0 -- 0 = ChannelAccessMode.All
FROM brands b
CROSS JOIN LATERAL (
    SELECT id FROM teams 
    WHERE workspace_id = b.workspace_id AND is_deleted = FALSE 
    ORDER BY created_at ASC 
    LIMIT 1
) t
WHERE b.is_deleted = FALSE
AND NOT EXISTS (
    SELECT 1 FROM team_brands tb 
    WHERE tb.team_id = t.id AND tb.brand_id = b.id AND tb.is_active = TRUE
);

-- 4. Cập nhật các bài viết (contents) chưa có team_id về team sở hữu brand tương ứng
UPDATE contents
SET team_id = tb.team_id
FROM team_brands tb
JOIN teams t ON t.id = tb.team_id
WHERE contents.brand_id = tb.brand_id
  AND contents.team_id IS NULL
  AND t.workspace_id = contents.workspace_id
  AND t.is_deleted = FALSE
  AND tb.is_active = TRUE;

-- 5. Cập nhật permission_revision cho các workspace vừa được cập nhật
UPDATE workspaces
SET permission_revision = permission_revision + 1
WHERE id IN (
    SELECT DISTINCT workspace_id FROM teams WHERE is_deleted = FALSE
);

COMMIT;
