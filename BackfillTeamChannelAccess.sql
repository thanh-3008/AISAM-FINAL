-- Migration: BackfillTeamChannelAccessCanView
-- Mục đích: Cấp quyền can_view=true cho tất cả TeamBrand đang active nhưng chưa có TeamChannelAccess
-- Idempotent: Sử dụng NOT EXISTS để chạy nhiều lần an toàn không tạo duplicate records.

INSERT INTO team_channel_access (id, team_brand_id, integration_id, can_view, can_publish, can_manage)
SELECT
    gen_random_uuid(),
    tb.id,
    si.id,
    true,   -- can_view = true (mặc định để member nhìn thấy kênh mạng xã hội của brand mình quản lý)
    false,  -- can_publish = false (cần Manager/Owner cấp riêng)
    false   -- can_manage = false (cần Manager/Owner cấp riêng)
FROM team_brands tb
JOIN social_integrations si ON si.brand_id = tb.brand_id
    AND si.workspace_id = (SELECT workspace_id FROM teams WHERE id = tb.team_id)
    AND si.is_deleted = false
WHERE tb.is_active = true
AND NOT EXISTS (
    SELECT 1 FROM team_channel_access tca
    WHERE tca.team_brand_id = tb.id
    AND tca.integration_id = si.id
);
