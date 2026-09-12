-- ==============================================================================
-- AISAM - PHASE 2 DATA MIGRATION RECONCILIATION SCRIPT
-- Mục đích: Đối soát toàn vẹn dữ liệu trước và sau khi chạy Phase 2 Migration
-- ==============================================================================

-- ------------------------------------------------------------------------------
-- 1. KIỂM TRA CONTENT THIẾU TEAM_ID (MỤC TIÊU: 0 BẢN GHI)
-- ------------------------------------------------------------------------------
SELECT 
    COUNT(*) AS total_contents,
    COUNT(team_id) AS contents_with_team,
    COUNT(*) - COUNT(team_id) AS contents_missing_team_id
FROM contents;

-- ------------------------------------------------------------------------------
-- 2. KIỂM TRA TOÀN VẸN WORKSPACE GIỮA CONTENT VÀ TEAM (MỤC TIÊU: 0 LỆCH)
-- ------------------------------------------------------------------------------
SELECT 
    COUNT(*) AS mismatched_workspace_contents
FROM contents c
JOIN teams t ON t.id = c.team_id
WHERE c.workspace_id <> t.workspace_id;

-- ------------------------------------------------------------------------------
-- 3. ĐỐI SOÁT VAI TRÒ WORKSPACEMEMBER (MỤC TIÊU: KHÔNG CÒN ROLE 2 CŨ, CHỈ CÒN 1 VÀ 3)
-- ------------------------------------------------------------------------------
SELECT 
    role,
    CASE 
        WHEN role = 1 THEN 'Owner'
        WHEN role = 2 THEN 'WorkspaceManager (NEW)'
        WHEN role = 3 THEN 'Member (NEW: includes legacy Manager, Creator, Viewer)'
        WHEN role = 4 THEN 'Legacy Viewer (SHOULD BE 0)'
        ELSE 'Unknown'
    END AS role_meaning,
    COUNT(*) AS member_count
FROM workspace_members
GROUP BY role
ORDER BY role;

-- ------------------------------------------------------------------------------
-- 4. BẢO ĐẢM MỌI LEGACY MANAGER (ROLE=2 CŨ) ĐÃ ĐƯỢC CẤP TEAM MANAGER (ROLE=1)
-- Mục tiêu: total_assigned_team_manager_count >= 1 cho mỗi user
-- ------------------------------------------------------------------------------
SELECT 
    b.workspace_id,
    b.user_id,
    b.old_role,
    wm.role AS current_workspace_role,
    COUNT(tm.id) AS total_assigned_team_manager_count
FROM _rbac_migration_wm_backup b
JOIN workspace_members wm ON wm.id = b.workspace_member_id
LEFT JOIN teams t ON t.workspace_id = b.workspace_id AND t.is_deleted = false AND t.status = 1
LEFT JOIN team_members tm ON tm.team_id = t.id AND tm.user_id = b.user_id AND tm.role = 1 AND tm.is_active = true
WHERE b.old_role = 2 -- Legacy Manager
GROUP BY b.workspace_id, b.user_id, b.old_role, wm.role;

-- ------------------------------------------------------------------------------
-- 5. KIỂM TRA BRAND ĐÃ CÓ ÍT NHẤT 1 TEAM NỐI (TEAM_BRANDS)
-- ------------------------------------------------------------------------------
SELECT 
    b.workspace_id,
    b.id AS brand_id,
    b.name AS brand_name,
    COUNT(tb.id) AS linked_active_teams
FROM brands b
LEFT JOIN team_brands tb ON tb.brand_id = b.id AND tb.is_active = true
WHERE b.is_deleted = false
GROUP BY b.workspace_id, b.id, b.name
HAVING COUNT(tb.id) = 0;
