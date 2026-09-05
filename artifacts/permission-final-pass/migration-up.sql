CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        email character varying(255) NOT NULL,
        role integer NOT NULL DEFAULT 0,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE assets (
        id uuid NOT NULL,
        uploaded_by uuid,
        type integer NOT NULL,
        storage_path text NOT NULL,
        mime_type character varying(100),
        size_bytes bigint,
        width integer,
        height integer,
        duration_seconds numeric(10,2),
        metadata jsonb,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_assets" PRIMARY KEY (id),
        CONSTRAINT "FK_assets_users_uploaded_by" FOREIGN KEY (uploaded_by) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE audit_logs (
        id uuid NOT NULL,
        actor_id uuid NOT NULL,
        action_type character varying(100) NOT NULL,
        target_table character varying(50) NOT NULL,
        target_id uuid NOT NULL,
        old_values jsonb,
        new_values jsonb,
        notes text,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_audit_logs" PRIMARY KEY (id),
        CONSTRAINT "FK_audit_logs_users_actor_id" FOREIGN KEY (actor_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE ad_campaigns (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        brand_id uuid NOT NULL,
        ad_account_id character varying(255) NOT NULL,
        facebook_campaign_id character varying(255),
        name character varying(255) NOT NULL,
        objective character varying(100),
        budget numeric(10,2),
        start_date date,
        end_date date,
        is_active boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ad_campaigns" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE ad_sets (
        id uuid NOT NULL,
        campaign_id uuid NOT NULL,
        name character varying(255) NOT NULL,
        facebook_ad_set_id character varying(255),
        targeting jsonb,
        daily_budget numeric(10,2),
        start_date date,
        end_date date,
        status character varying(50),
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        "AdCampaignId" uuid,
        CONSTRAINT "PK_ad_sets" PRIMARY KEY (id),
        CONSTRAINT "FK_ad_sets_ad_campaigns_AdCampaignId" FOREIGN KEY ("AdCampaignId") REFERENCES ad_campaigns (id),
        CONSTRAINT "FK_ad_sets_ad_campaigns_campaign_id" FOREIGN KEY (campaign_id) REFERENCES ad_campaigns (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE ad_creatives (
        id uuid NOT NULL,
        content_id uuid,
        ad_account_id character varying(255) NOT NULL,
        creative_id character varying(255),
        call_to_action character varying(50),
        facebook_post_id character varying(255),
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ad_creatives" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE ads (
        id uuid NOT NULL,
        ad_set_id uuid NOT NULL,
        creative_id uuid NOT NULL,
        ad_id character varying(255),
        status character varying(50),
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ads" PRIMARY KEY (id),
        CONSTRAINT "FK_ads_ad_creatives_creative_id" FOREIGN KEY (creative_id) REFERENCES ad_creatives (id) ON DELETE CASCADE,
        CONSTRAINT "FK_ads_ad_sets_ad_set_id" FOREIGN KEY (ad_set_id) REFERENCES ad_sets (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE ai_generations (
        id uuid NOT NULL,
        content_id uuid NOT NULL,
        ai_prompt text NOT NULL,
        generated_text text,
        generated_image_url character varying(500),
        generated_video_url character varying(500),
        status integer NOT NULL DEFAULT 0,
        error_message text,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ai_generations" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE approvals (
        id uuid NOT NULL,
        content_id uuid NOT NULL,
        approver_profile_id uuid,
        approver_user_id uuid NOT NULL,
        status integer NOT NULL,
        notes text,
        approved_at timestamp with time zone,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_approvals" PRIMARY KEY (id),
        CONSTRAINT "FK_approvals_users_approver_user_id" FOREIGN KEY (approver_user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE brands (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        name character varying(255) NOT NULL,
        description text,
        logo_url character varying(500),
        slogan character varying(255),
        usp text,
        target_audience text,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_brands" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE content_templates (
        id uuid NOT NULL,
        brand_id uuid NOT NULL,
        name character varying(255) NOT NULL,
        template_type character varying(50) NOT NULL,
        template_data jsonb NOT NULL,
        representative_character text,
        is_active boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_content_templates" PRIMARY KEY (id),
        CONSTRAINT "FK_content_templates_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE products (
        id uuid NOT NULL,
        brand_id uuid NOT NULL,
        name character varying(255) NOT NULL,
        description text,
        price numeric(10,2),
        images jsonb,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_products" PRIMARY KEY (id),
        CONSTRAINT "FK_products_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE chat_messages (
        id uuid NOT NULL,
        conversation_id uuid NOT NULL,
        sender_type integer NOT NULL,
        message text NOT NULL,
        ai_generation_id uuid,
        content_id uuid,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_chat_messages" PRIMARY KEY (id),
        CONSTRAINT "FK_chat_messages_ai_generations_ai_generation_id" FOREIGN KEY (ai_generation_id) REFERENCES ai_generations (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE content_calendar (
        id uuid NOT NULL,
        content_id uuid NOT NULL,
        scheduled_date timestamp with time zone NOT NULL,
        scheduled_time interval,
        timezone character varying(50) NOT NULL,
        repeat_type integer NOT NULL,
        repeat_interval integer NOT NULL,
        repeat_until timestamp with time zone,
        next_scheduled_date timestamp with time zone,
        integration_ids text,
        profile_id uuid NOT NULL,
        is_active boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_content_calendar" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE contents (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        brand_id uuid NOT NULL,
        product_id uuid,
        ad_type integer NOT NULL,
        title character varying(255),
        text_content text NOT NULL,
        image_url jsonb,
        video_url character varying(500),
        style_description text,
        context_description text,
        representative_character text,
        status integer NOT NULL DEFAULT 0,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_contents" PRIMARY KEY (id),
        CONSTRAINT "FK_contents_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE,
        CONSTRAINT "FK_contents_products_product_id" FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE conversations (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        brand_id uuid,
        product_id uuid,
        ad_type integer NOT NULL,
        title character varying(255),
        is_active boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_conversations" PRIMARY KEY (id),
        CONSTRAINT "FK_conversations_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE SET NULL,
        CONSTRAINT "FK_conversations_products_product_id" FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE notifications (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        title character varying(255) NOT NULL,
        message text NOT NULL,
        type integer NOT NULL,
        target_id uuid,
        target_type character varying(50),
        is_read boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_notifications" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE payments (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        subscription_id uuid,
        amount numeric(10,2) NOT NULL,
        currency character varying(3) NOT NULL,
        status integer NOT NULL DEFAULT 0,
        payment_method character varying(50),
        transaction_id character varying(255),
        invoice_url character varying(500),
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_payments" PRIMARY KEY (id),
        CONSTRAINT "FK_payments_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE performance_reports (
        id uuid NOT NULL,
        post_id uuid,
        ad_id uuid,
        impressions bigint NOT NULL,
        engagement bigint NOT NULL,
        ctr numeric(5,4) NOT NULL,
        estimated_revenue numeric(10,2) NOT NULL,
        report_date date NOT NULL,
        raw_data jsonb,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_performance_reports" PRIMARY KEY (id),
        CONSTRAINT "FK_performance_reports_ads_ad_id" FOREIGN KEY (ad_id) REFERENCES ads (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE posts (
        id uuid NOT NULL,
        content_id uuid NOT NULL,
        integration_id uuid NOT NULL,
        external_post_id character varying(255),
        published_at timestamp with time zone NOT NULL,
        status integer NOT NULL DEFAULT 4,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        "SocialIntegrationId" uuid,
        CONSTRAINT "PK_posts" PRIMARY KEY (id),
        CONSTRAINT "FK_posts_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE profiles (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        name character varying(255) NOT NULL,
        profile_type integer NOT NULL,
        subscription_id uuid,
        company_name character varying(255),
        bio text,
        avatar_url character varying(500),
        status integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_profiles" PRIMARY KEY (id),
        CONSTRAINT "FK_profiles_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE social_accounts (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        platform integer NOT NULL,
        account_id character varying(255),
        user_access_token text NOT NULL,
        refresh_token text,
        expires_at timestamp with time zone,
        is_active boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_social_accounts" PRIMARY KEY (id),
        CONSTRAINT "FK_social_accounts_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE subscriptions (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        plan integer NOT NULL,
        quota_posts_per_month integer NOT NULL,
        quota_storage_gb integer NOT NULL,
        quota_ad_budget_monthly numeric(10,2) NOT NULL,
        quota_ad_campaigns integer NOT NULL,
        start_date date NOT NULL,
        end_date date,
        is_active boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        stripe_subscription_id character varying(255),
        stripe_customer_id character varying(255),
        CONSTRAINT "PK_subscriptions" PRIMARY KEY (id),
        CONSTRAINT "FK_subscriptions_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE teams (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        name character varying(255) NOT NULL,
        description character varying(1000),
        is_deleted boolean NOT NULL,
        status integer NOT NULL DEFAULT 0,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_teams" PRIMARY KEY (id),
        CONSTRAINT "FK_teams_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE social_integrations (
        id uuid NOT NULL,
        profile_id uuid NOT NULL,
        brand_id uuid NOT NULL,
        social_account_id uuid NOT NULL,
        platform integer NOT NULL,
        access_token text NOT NULL,
        refresh_token text,
        expires_at timestamp with time zone,
        external_id character varying(255),
        ad_account_id character varying(255),
        is_active boolean NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_social_integrations" PRIMARY KEY (id),
        CONSTRAINT "FK_social_integrations_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE,
        CONSTRAINT "FK_social_integrations_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE,
        CONSTRAINT "FK_social_integrations_social_accounts_social_account_id" FOREIGN KEY (social_account_id) REFERENCES social_accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE team_brands (
        id uuid NOT NULL,
        team_id uuid NOT NULL,
        brand_id uuid NOT NULL,
        assigned_at timestamp with time zone NOT NULL,
        is_active boolean NOT NULL,
        CONSTRAINT "PK_team_brands" PRIMARY KEY (id),
        CONSTRAINT "FK_team_brands_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE,
        CONSTRAINT "FK_team_brands_teams_team_id" FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE TABLE team_members (
        id uuid NOT NULL,
        team_id uuid NOT NULL,
        user_id uuid NOT NULL,
        role character varying(100) NOT NULL,
        permissions jsonb NOT NULL,
        joined_at timestamp with time zone NOT NULL,
        is_active boolean NOT NULL,
        CONSTRAINT "PK_team_members" PRIMARY KEY (id),
        CONSTRAINT "FK_team_members_teams_team_id" FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE CASCADE,
        CONSTRAINT "FK_team_members_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ad_campaigns_brand_id" ON ad_campaigns (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ad_campaigns_name" ON ad_campaigns (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ad_campaigns_profile_id" ON ad_campaigns (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ad_creatives_content_id" ON ad_creatives (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ad_sets_AdCampaignId" ON ad_sets ("AdCampaignId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ad_sets_campaign_id" ON ad_sets (campaign_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ads_ad_set_id" ON ads (ad_set_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ads_creative_id" ON ads (creative_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ai_generations_content_id" ON ai_generations (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_ai_generations_status" ON ai_generations (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_approvals_approver_profile_id" ON approvals (approver_profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_approvals_approver_user_id" ON approvals (approver_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_approvals_content_id" ON approvals (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_assets_uploaded_by" ON assets (uploaded_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_audit_logs_actor_id" ON audit_logs (actor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_audit_logs_created_at" ON audit_logs (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_audit_logs_target_table" ON audit_logs (target_table);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_brands_name" ON brands (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_brands_profile_id" ON brands (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_chat_messages_ai_generation_id" ON chat_messages (ai_generation_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_chat_messages_content_id" ON chat_messages (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_chat_messages_conversation_id" ON chat_messages (conversation_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_chat_messages_created_at" ON chat_messages (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_content_calendar_content_id" ON content_calendar (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_content_calendar_profile_id" ON content_calendar (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_content_calendar_scheduled_date" ON content_calendar (scheduled_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_content_templates_brand_id" ON content_templates (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_content_templates_is_active" ON content_templates (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_content_templates_template_type" ON content_templates (template_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_contents_brand_id" ON contents (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_contents_created_at" ON contents (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_contents_product_id" ON contents (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_contents_profile_id" ON contents (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_contents_status" ON contents (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_conversations_brand_id" ON conversations (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_conversations_created_at" ON conversations (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_conversations_is_active" ON conversations (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_conversations_product_id" ON conversations (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_conversations_profile_id" ON conversations (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_notifications_created_at" ON notifications (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_notifications_is_read" ON notifications (is_read);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_notifications_profile_id" ON notifications (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_notifications_type" ON notifications (type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_payments_status" ON payments (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_payments_subscription_id" ON payments (subscription_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_payments_user_id" ON payments (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_performance_reports_ad_id" ON performance_reports (ad_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_performance_reports_post_id" ON performance_reports (post_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_performance_reports_report_date" ON performance_reports (report_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_posts_content_id" ON posts (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_posts_external_post_id" ON posts (external_post_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_posts_integration_id" ON posts (integration_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_posts_published_at" ON posts (published_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_posts_SocialIntegrationId" ON posts ("SocialIntegrationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_products_brand_id" ON products (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_products_name" ON products (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_profiles_subscription_id" ON profiles (subscription_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_profiles_user_id" ON profiles (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_social_accounts_account_id" ON social_accounts (account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_social_accounts_is_active" ON social_accounts (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_social_accounts_platform" ON social_accounts (platform);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_social_accounts_profile_id" ON social_accounts (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_social_integrations_brand_id" ON social_integrations (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_social_integrations_profile_id" ON social_integrations (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_social_integrations_social_account_id" ON social_integrations (social_account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_subscriptions_is_active" ON subscriptions (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_subscriptions_profile_id" ON subscriptions (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_team_brands_brand_id" ON team_brands (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_team_brands_is_active" ON team_brands (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_team_brands_team_id" ON team_brands (team_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_team_members_team_id" ON team_members (team_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_team_members_user_id" ON team_members (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_teams_name" ON teams (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_teams_profile_id" ON teams (profile_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_teams_status" ON teams (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_users_email" ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    CREATE INDEX "IX_users_role" ON users (role);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE ad_campaigns ADD CONSTRAINT "FK_ad_campaigns_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE ad_campaigns ADD CONSTRAINT "FK_ad_campaigns_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE ad_creatives ADD CONSTRAINT "FK_ad_creatives_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE ai_generations ADD CONSTRAINT "FK_ai_generations_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE approvals ADD CONSTRAINT "FK_approvals_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE approvals ADD CONSTRAINT "FK_approvals_profiles_approver_profile_id" FOREIGN KEY (approver_profile_id) REFERENCES profiles (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE brands ADD CONSTRAINT "FK_brands_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE chat_messages ADD CONSTRAINT "FK_chat_messages_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE chat_messages ADD CONSTRAINT "FK_chat_messages_conversations_conversation_id" FOREIGN KEY (conversation_id) REFERENCES conversations (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE content_calendar ADD CONSTRAINT "FK_content_calendar_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE content_calendar ADD CONSTRAINT "FK_content_calendar_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE contents ADD CONSTRAINT "FK_contents_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE conversations ADD CONSTRAINT "FK_conversations_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE notifications ADD CONSTRAINT "FK_notifications_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE payments ADD CONSTRAINT "FK_payments_subscriptions_subscription_id" FOREIGN KEY (subscription_id) REFERENCES subscriptions (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE performance_reports ADD CONSTRAINT "FK_performance_reports_posts_post_id" FOREIGN KEY (post_id) REFERENCES posts (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE posts ADD CONSTRAINT "FK_posts_social_integrations_SocialIntegrationId" FOREIGN KEY ("SocialIntegrationId") REFERENCES social_integrations (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE posts ADD CONSTRAINT "FK_posts_social_integrations_integration_id" FOREIGN KEY (integration_id) REFERENCES social_integrations (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    ALTER TABLE profiles ADD CONSTRAINT "FK_profiles_subscriptions_subscription_id" FOREIGN KEY (subscription_id) REFERENCES subscriptions (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102025736_Initial') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251102025736_Initial', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    DROP INDEX "IX_users_email";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    ALTER TABLE users ADD full_name character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    ALTER TABLE users ADD is_email_verified boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    ALTER TABLE users ADD last_login_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    ALTER TABLE users ADD password_hash character varying(500) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    ALTER TABLE users ADD password_salt character varying(100) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    ALTER TABLE users ADD updated_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    ALTER TABLE ad_creatives ADD link_url character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    CREATE TABLE sessions (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        refresh_token character varying(500) NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        created_at timestamp with time zone NOT NULL,
        revoked_at timestamp with time zone,
        user_agent character varying(500),
        ip_address character varying(50),
        is_active boolean NOT NULL,
        CONSTRAINT "PK_sessions" PRIMARY KEY (id),
        CONSTRAINT "FK_sessions_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    CREATE INDEX "IX_users_created_at" ON users (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    CREATE UNIQUE INDEX "IX_users_email" ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    CREATE INDEX "IX_sessions_expires_at" ON sessions (expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    CREATE INDEX "IX_sessions_is_active" ON sessions (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    CREATE INDEX "IX_sessions_refresh_token" ON sessions (refresh_token);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    CREATE INDEX "IX_sessions_user_id" ON sessions (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124120929_AddCustomAuthenticationTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260124120929_AddCustomAuthenticationTables', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124133308_verifytoken') THEN
    ALTER TABLE users ADD email_verification_token character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124133308_verifytoken') THEN
    ALTER TABLE users ADD email_verification_token_expires_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124133308_verifytoken') THEN
    ALTER TABLE users ADD password_reset_token character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124133308_verifytoken') THEN
    ALTER TABLE users ADD password_reset_token_expires_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124133308_verifytoken') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260124133308_verifytoken', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124135926_UpdatePasswordSaltLength') THEN
    ALTER TABLE users ALTER COLUMN password_salt TYPE character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260124135926_UpdatePasswordSaltLength') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260124135926_UpdatePasswordSaltLength', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions RENAME COLUMN stripe_subscription_id TO payos_payment_link_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions RENAME COLUMN stripe_customer_id TO payos_order_code;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions RENAME COLUMN quota_storage_gb TO quota_platforms;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions ALTER COLUMN quota_ad_budget_monthly TYPE numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions ADD analysis_level integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions ADD quota_accounts integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions ADD quota_ai_content_per_day integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    ALTER TABLE subscriptions ADD quota_ai_images_per_day integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260127160619_UpdateSubscriptionPayOS') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260127160619_UpdateSubscriptionPayOS', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531161937_RemovePostSocialIntegrationShadowFk') THEN
    ALTER TABLE posts DROP CONSTRAINT "FK_posts_social_integrations_SocialIntegrationId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531161937_RemovePostSocialIntegrationShadowFk') THEN
    DROP INDEX "IX_posts_SocialIntegrationId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531161937_RemovePostSocialIntegrationShadowFk') THEN
    ALTER TABLE posts DROP COLUMN "SocialIntegrationId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531161937_RemovePostSocialIntegrationShadowFk') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260531161937_RemovePostSocialIntegrationShadowFk', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    ALTER TABLE content_calendar ADD attempt_count integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    ALTER TABLE content_calendar ADD executed_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    ALTER TABLE content_calendar ADD integration_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    ALTER TABLE content_calendar ADD last_error text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    ALTER TABLE content_calendar ADD scheduled_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    ALTER TABLE content_calendar ADD status integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    CREATE INDEX "IX_content_calendar_integration_id" ON content_calendar (integration_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    CREATE INDEX "IX_content_calendar_scheduled_at" ON content_calendar (scheduled_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    CREATE INDEX "IX_content_calendar_status" ON content_calendar (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    ALTER TABLE content_calendar ADD CONSTRAINT "FK_content_calendar_social_integrations_integration_id" FOREIGN KEY (integration_id) REFERENCES social_integrations (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601095652_AddContentCalendarSchedulingRuntimeFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260601095652_AddContentCalendarSchedulingRuntimeFields', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE TABLE workspaces (
        id uuid NOT NULL,
        name character varying(255) NOT NULL,
        workspace_type integer NOT NULL,
        status integer NOT NULL DEFAULT 1,
        subscription_expired_at timestamp with time zone,
        archived_at timestamp with time zone,
        deleted_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_workspaces" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE TABLE workspace_members (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        user_id uuid NOT NULL,
        role integer NOT NULL,
        quota_mode integer NOT NULL DEFAULT 1,
        credit_limit bigint,
        credit_used bigint NOT NULL,
        credit_period_start date,
        joined_at timestamp with time zone NOT NULL,
        is_active boolean NOT NULL,
        CONSTRAINT "PK_workspace_members" PRIMARY KEY (id),
        CONSTRAINT "FK_workspace_members_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
        CONSTRAINT "FK_workspace_members_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspace_members_is_active" ON workspace_members (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspace_members_user_id" ON workspace_members (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspace_members_workspace_id" ON workspace_members (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspace_members_workspace_id_role" ON workspace_members (workspace_id, role);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE UNIQUE INDEX "IX_workspace_members_workspace_id_user_id" ON workspace_members (workspace_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspaces_archived_at" ON workspaces (archived_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspaces_deleted_at" ON workspaces (deleted_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspaces_status" ON workspaces (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspaces_subscription_expired_at" ON workspaces (subscription_expired_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    CREATE INDEX "IX_workspaces_workspace_type" ON workspaces (workspace_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610064359_AddWorkspaceFoundation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260610064359_AddWorkspaceFoundation', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610160919_AddWorkspaceInvitationFoundation') THEN
    CREATE TABLE workspace_invitations (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        email character varying(255) NOT NULL,
        role integer NOT NULL,
        token character varying(500) NOT NULL,
        invited_by_user_id uuid NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        accepted_at timestamp with time zone,
        revoked_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_workspace_invitations" PRIMARY KEY (id),
        CONSTRAINT "FK_workspace_invitations_users_invited_by_user_id" FOREIGN KEY (invited_by_user_id) REFERENCES users (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_workspace_invitations_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610160919_AddWorkspaceInvitationFoundation') THEN
    CREATE INDEX "IX_workspace_invitations_expires_at" ON workspace_invitations (expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610160919_AddWorkspaceInvitationFoundation') THEN
    CREATE INDEX "IX_workspace_invitations_invited_by_user_id" ON workspace_invitations (invited_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610160919_AddWorkspaceInvitationFoundation') THEN
    CREATE UNIQUE INDEX "IX_workspace_invitations_token" ON workspace_invitations (token);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610160919_AddWorkspaceInvitationFoundation') THEN
    CREATE INDEX "IX_workspace_invitations_workspace_id_email" ON workspace_invitations (workspace_id, email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610160919_AddWorkspaceInvitationFoundation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260610160919_AddWorkspaceInvitationFoundation', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610172441_AddWorkspaceMemberLimit') THEN
    ALTER TABLE workspaces ADD member_limit integer NOT NULL DEFAULT 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610172441_AddWorkspaceMemberLimit') THEN
    UPDATE workspaces SET member_limit = 10 WHERE workspace_type = 2;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610172441_AddWorkspaceMemberLimit') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260610172441_AddWorkspaceMemberLimit', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611085418_EnforceSingleActiveWorkspaceOwner') THEN
    DROP INDEX "IX_workspace_members_workspace_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611085418_EnforceSingleActiveWorkspaceOwner') THEN
    CREATE UNIQUE INDEX "IX_workspace_members_workspace_id" ON workspace_members (workspace_id) WHERE "role" = 1 AND "is_active" = TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611085418_EnforceSingleActiveWorkspaceOwner') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611085418_EnforceSingleActiveWorkspaceOwner', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    ALTER TABLE subscriptions DROP CONSTRAINT "FK_subscriptions_profiles_profile_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    ALTER TABLE subscriptions ALTER COLUMN profile_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    ALTER TABLE subscriptions ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    ALTER TABLE payments ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    CREATE INDEX "IX_subscriptions_workspace_id" ON subscriptions (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    CREATE INDEX "IX_payments_workspace_id" ON payments (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    ALTER TABLE payments ADD CONSTRAINT "FK_payments_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    ALTER TABLE subscriptions ADD CONSTRAINT "FK_subscriptions_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    ALTER TABLE subscriptions ADD CONSTRAINT "FK_subscriptions_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611092549_AddWorkspacePaymentSubscriptionOwnership') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611092549_AddWorkspacePaymentSubscriptionOwnership', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    CREATE TABLE credit_usage_records (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        user_id uuid NOT NULL,
        ai_generation_id uuid,
        action integer NOT NULL,
        credits bigint NOT NULL,
        status integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_credit_usage_records" PRIMARY KEY (id),
        CONSTRAINT "FK_credit_usage_records_ai_generations_ai_generation_id" FOREIGN KEY (ai_generation_id) REFERENCES ai_generations (id) ON DELETE SET NULL,
        CONSTRAINT "FK_credit_usage_records_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
        CONSTRAINT "FK_credit_usage_records_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    CREATE TABLE credit_wallets (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        balance bigint NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_credit_wallets" PRIMARY KEY (id),
        CONSTRAINT "FK_credit_wallets_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    CREATE INDEX "IX_credit_usage_records_ai_generation_id" ON credit_usage_records (ai_generation_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    CREATE INDEX "IX_credit_usage_records_created_at" ON credit_usage_records (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    CREATE INDEX "IX_credit_usage_records_user_id" ON credit_usage_records (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    CREATE INDEX "IX_credit_usage_records_workspace_id" ON credit_usage_records (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    CREATE UNIQUE INDEX "IX_credit_wallets_workspace_id" ON credit_wallets (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611115818_AddCreditWalletAndUsageTracking') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611115818_AddCreditWalletAndUsageTracking', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611123701_AddCreditPackPaymentType') THEN
    ALTER TABLE payments ADD credit_amount bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611123701_AddCreditPackPaymentType') THEN
    ALTER TABLE payments ADD credit_pack_code integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611123701_AddCreditPackPaymentType') THEN
    ALTER TABLE payments ADD payment_type integer NOT NULL DEFAULT 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611123701_AddCreditPackPaymentType') THEN
    CREATE INDEX "IX_payments_payment_type" ON payments (payment_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611123701_AddCreditPackPaymentType') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611123701_AddCreditPackPaymentType', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611131708_AddWorkspaceInvitationQuotaModes') THEN
    ALTER TABLE workspace_invitations ADD credit_limit bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611131708_AddWorkspaceInvitationQuotaModes') THEN
    ALTER TABLE workspace_invitations ADD quota_mode integer NOT NULL DEFAULT 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611131708_AddWorkspaceInvitationQuotaModes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611131708_AddWorkspaceInvitationQuotaModes', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612020911_FixEfModelConfigurationWarnings') THEN
    DROP INDEX "IX_profiles_subscription_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612020911_FixEfModelConfigurationWarnings') THEN
    CREATE UNIQUE INDEX "IX_profiles_subscription_id" ON profiles (subscription_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612020911_FixEfModelConfigurationWarnings') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260612020911_FixEfModelConfigurationWarnings', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612024207_AddBrandWorkspaceOwnership') THEN
    ALTER TABLE brands ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612024207_AddBrandWorkspaceOwnership') THEN
    CREATE INDEX "IX_brands_workspace_id" ON brands (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612024207_AddBrandWorkspaceOwnership') THEN
    ALTER TABLE brands ADD CONSTRAINT "FK_brands_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612024207_AddBrandWorkspaceOwnership') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260612024207_AddBrandWorkspaceOwnership', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE social_integrations ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE social_accounts ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE notifications ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE conversations ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE contents ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE content_calendar ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE ad_campaigns ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    CREATE INDEX "IX_social_integrations_workspace_id" ON social_integrations (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    CREATE INDEX "IX_social_accounts_workspace_id" ON social_accounts (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    CREATE INDEX "IX_notifications_workspace_id" ON notifications (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    CREATE INDEX "IX_conversations_workspace_id" ON conversations (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    CREATE INDEX "IX_contents_workspace_id" ON contents (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    CREATE INDEX "IX_content_calendar_workspace_id" ON content_calendar (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    CREATE INDEX "IX_ad_campaigns_workspace_id" ON ad_campaigns (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE ad_campaigns ADD CONSTRAINT "FK_ad_campaigns_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE content_calendar ADD CONSTRAINT "FK_content_calendar_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE contents ADD CONSTRAINT "FK_contents_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE conversations ADD CONSTRAINT "FK_conversations_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE notifications ADD CONSTRAINT "FK_notifications_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE social_accounts ADD CONSTRAINT "FK_social_accounts_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    ALTER TABLE social_integrations ADD CONSTRAINT "FK_social_integrations_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613011615_AddRemainingDomainWorkspaceOwnership') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260613011615_AddRemainingDomainWorkspaceOwnership', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    CREATE TEMP TABLE legacy_profile_workspace_map
    (
        profile_id uuid PRIMARY KEY,
        user_id uuid NOT NULL,
        workspace_id uuid NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO legacy_profile_workspace_map (profile_id, user_id, workspace_id)
    SELECT
        p.id,
        p.user_id,
        COALESCE(
            CASE WHEN profile_stats.profile_count = 1 THEN
                (
                    SELECT CASE WHEN COUNT(*) = 1 THEN MIN(w.id::text)::uuid END
                    FROM workspace_members wm
                    JOIN workspaces w ON w.id = wm.workspace_id
                    WHERE wm.user_id = p.user_id
                      AND wm.role = 1
                      AND wm.is_active = TRUE
                      AND w.workspace_type = 1
                      AND w.status <> 5
                )
            END,
            md5('aisam-legacy-profile-workspace:' || p.id::text)::uuid
        )
    FROM profiles p
    JOIN (
        SELECT user_id, COUNT(*) AS profile_count
        FROM profiles
        GROUP BY user_id
    ) profile_stats ON profile_stats.user_id = p.user_id;

    INSERT INTO workspaces
        (id, name, workspace_type, status, member_limit, created_at, updated_at)
    SELECT
        map.workspace_id,
        LEFT(COALESCE(NULLIF(TRIM(p.name), ''), 'Personal') || ' Workspace', 255),
        1,
        1,
        1,
        COALESCE(p.created_at, CURRENT_TIMESTAMP),
        CURRENT_TIMESTAMP
    FROM legacy_profile_workspace_map map
    JOIN profiles p ON p.id = map.profile_id
    WHERE NOT EXISTS (SELECT 1 FROM workspaces w WHERE w.id = map.workspace_id);

    INSERT INTO workspace_members
        (id, workspace_id, user_id, role, quota_mode, credit_used, joined_at, is_active)
    SELECT
        md5('aisam-legacy-profile-owner:' || map.profile_id::text)::uuid,
        map.workspace_id,
        map.user_id,
        1,
        1,
        0,
        CURRENT_TIMESTAMP,
        TRUE
    FROM legacy_profile_workspace_map map
    WHERE NOT EXISTS (
        SELECT 1 FROM workspace_members wm
        WHERE wm.workspace_id = map.workspace_id AND wm.user_id = map.user_id
    );

    CREATE TEMP TABLE legacy_payment_workspace_map
    (
        user_id uuid PRIMARY KEY,
        workspace_id uuid NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO legacy_payment_workspace_map (user_id, workspace_id)
    SELECT
        u.id,
        COALESCE(
            (SELECT MIN(map.workspace_id::text)::uuid FROM legacy_profile_workspace_map map WHERE map.user_id = u.id),
            md5('aisam-legacy-user-workspace:' || u.id::text)::uuid
        )
    FROM users u
    WHERE EXISTS (SELECT 1 FROM payments payment WHERE payment.user_id = u.id AND payment.workspace_id IS NULL);

    INSERT INTO workspaces
        (id, name, workspace_type, status, member_limit, created_at, updated_at)
    SELECT
        map.workspace_id,
        LEFT(COALESCE(NULLIF(TRIM(u.full_name), ''), 'Personal') || ' Workspace', 255),
        1,
        1,
        1,
        COALESCE(u.created_at, CURRENT_TIMESTAMP),
        CURRENT_TIMESTAMP
    FROM legacy_payment_workspace_map map
    JOIN users u ON u.id = map.user_id
    WHERE NOT EXISTS (SELECT 1 FROM workspaces w WHERE w.id = map.workspace_id);

    INSERT INTO workspace_members
        (id, workspace_id, user_id, role, quota_mode, credit_used, joined_at, is_active)
    SELECT
        md5('aisam-legacy-user-owner:' || map.user_id::text)::uuid,
        map.workspace_id,
        map.user_id,
        1,
        1,
        0,
        CURRENT_TIMESTAMP,
        TRUE
    FROM legacy_payment_workspace_map map
    WHERE NOT EXISTS (
        SELECT 1 FROM workspace_members wm
        WHERE wm.workspace_id = map.workspace_id AND wm.user_id = map.user_id
    );

    UPDATE subscriptions entity
    SET workspace_id = map.workspace_id
    FROM legacy_profile_workspace_map map
    WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

    UPDATE brands entity
    SET workspace_id = map.workspace_id
    FROM legacy_profile_workspace_map map
    WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

    UPDATE contents entity
    SET workspace_id = COALESCE(brand.workspace_id, map.workspace_id)
    FROM legacy_profile_workspace_map map
    LEFT JOIN brands brand ON brand.profile_id = map.profile_id
    WHERE entity.workspace_id IS NULL
      AND entity.profile_id = map.profile_id
      AND (brand.id IS NULL OR brand.id = entity.brand_id);

    UPDATE social_accounts entity
    SET workspace_id = map.workspace_id
    FROM legacy_profile_workspace_map map
    WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

    UPDATE social_integrations entity
    SET workspace_id = COALESCE(account.workspace_id, brand.workspace_id, map.workspace_id)
    FROM legacy_profile_workspace_map map
    LEFT JOIN social_accounts account ON account.profile_id = map.profile_id
    LEFT JOIN brands brand ON brand.profile_id = map.profile_id
    WHERE entity.workspace_id IS NULL
      AND entity.profile_id = map.profile_id
      AND (account.id IS NULL OR account.id = entity.social_account_id)
      AND (brand.id IS NULL OR brand.id = entity.brand_id);

    UPDATE content_calendar entity
    SET workspace_id = COALESCE(content.workspace_id, map.workspace_id)
    FROM legacy_profile_workspace_map map
    LEFT JOIN contents content ON content.profile_id = map.profile_id
    WHERE entity.workspace_id IS NULL
      AND entity.profile_id = map.profile_id
      AND (content.id IS NULL OR content.id = entity.content_id);

    UPDATE conversations entity
    SET workspace_id = COALESCE(brand.workspace_id, map.workspace_id)
    FROM legacy_profile_workspace_map map
    LEFT JOIN brands brand ON brand.profile_id = map.profile_id
    WHERE entity.workspace_id IS NULL
      AND entity.profile_id = map.profile_id
      AND (brand.id IS NULL OR brand.id = entity.brand_id);

    UPDATE notifications entity
    SET workspace_id = map.workspace_id
    FROM legacy_profile_workspace_map map
    WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

    UPDATE ad_campaigns entity
    SET workspace_id = COALESCE(brand.workspace_id, map.workspace_id)
    FROM legacy_profile_workspace_map map
    LEFT JOIN brands brand ON brand.profile_id = map.profile_id
    WHERE entity.workspace_id IS NULL
      AND entity.profile_id = map.profile_id
      AND (brand.id IS NULL OR brand.id = entity.brand_id);

    UPDATE payments entity
    SET workspace_id = subscription.workspace_id
    FROM subscriptions subscription
    WHERE entity.workspace_id IS NULL
      AND entity.subscription_id = subscription.id;

    UPDATE payments entity
    SET workspace_id = map.workspace_id
    FROM legacy_payment_workspace_map map
    WHERE entity.workspace_id IS NULL AND entity.user_id = map.user_id;

    INSERT INTO credit_wallets (id, workspace_id, balance, created_at, updated_at)
    SELECT
        md5('aisam-legacy-workspace-wallet:' || w.id::text)::uuid,
        w.id,
        0,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    FROM workspaces w
    WHERE NOT EXISTS (SELECT 1 FROM credit_wallets wallet WHERE wallet.workspace_id = w.id);

    DO $$
    DECLARE
        missing_count bigint;
    BEGIN
        SELECT
            (SELECT COUNT(*) FROM subscriptions WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM payments WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM brands WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM contents WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM social_accounts WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM social_integrations WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM content_calendar WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM conversations WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM notifications WHERE workspace_id IS NULL) +
            (SELECT COUNT(*) FROM ad_campaigns WHERE workspace_id IS NULL)
        INTO missing_count;

        IF missing_count > 0 THEN
            RAISE EXCEPTION 'Workspace backfill left % ownership rows unmapped', missing_count;
        END IF;
    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE subscriptions ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE social_integrations ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE social_accounts ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE payments ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE notifications ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE conversations ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE contents ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE content_calendar ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE brands ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    ALTER TABLE ad_campaigns ALTER COLUMN workspace_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613130339_ProvisionMissingPersonalFreePlan') THEN
    INSERT INTO subscriptions (
        id, profile_id, workspace_id, plan,
        quota_posts_per_month, quota_ai_content_per_day, quota_ai_images_per_day,
        quota_platforms, quota_accounts, analysis_level, quota_ad_budget_monthly, quota_ad_campaigns,
        start_date, end_date, is_active, is_deleted, created_at, updated_at,
        payos_order_code, payos_payment_link_id)
    SELECT
        md5('aisam-personal-free-subscription:' || workspace.id::text)::uuid,
        NULL,
        workspace.id,
        0,
        20, 0, 0, 1, 1, 0, 0, 0,
        CURRENT_DATE,
        NULL,
        TRUE,
        FALSE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        NULL
    FROM workspaces workspace
    WHERE workspace.workspace_type = 1
      AND workspace.status <> 5
      AND NOT EXISTS (
          SELECT 1 FROM subscriptions subscription
          WHERE subscription.workspace_id = workspace.id
      );

    UPDATE credit_wallets wallet
    SET balance = 50,
        updated_at = CURRENT_TIMESTAMP
    WHERE EXISTS (
        SELECT 1 FROM subscriptions subscription
        WHERE subscription.workspace_id = wallet.workspace_id
          AND subscription.plan = 0
          AND subscription.is_active = TRUE
          AND subscription.is_deleted = FALSE
    )
      AND NOT EXISTS (
        SELECT 1 FROM credit_usage_records usage
        WHERE usage.workspace_id = wallet.workspace_id
          AND usage.action = 1
          AND usage.status = 2
    );

    INSERT INTO credit_usage_records (
        id, workspace_id, user_id, ai_generation_id, action, credits, status, created_at)
    SELECT
        md5('aisam-personal-free-credit-grant:' || workspace.id::text)::uuid,
        workspace.id,
        owner.user_id,
        NULL,
        1,
        50,
        2,
        CURRENT_TIMESTAMP
    FROM workspaces workspace
    JOIN workspace_members owner
      ON owner.workspace_id = workspace.id
     AND owner.role = 1
     AND owner.is_active = TRUE
    WHERE EXISTS (
        SELECT 1 FROM subscriptions subscription
        WHERE subscription.workspace_id = workspace.id
          AND subscription.plan = 0
          AND subscription.is_active = TRUE
          AND subscription.is_deleted = FALSE
    )
      AND NOT EXISTS (
        SELECT 1 FROM credit_usage_records usage
        WHERE usage.workspace_id = workspace.id
          AND usage.action = 1
          AND usage.status = 2
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260613130339_ProvisionMissingPersonalFreePlan') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260613130339_ProvisionMissingPersonalFreePlan', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260620153457_AddProductStock') THEN
    ALTER TABLE products ADD stock integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260620153457_AddProductStock') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260620153457_AddProductStock', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260623151611_RemoveAdSetShadowForeignKey') THEN
    ALTER TABLE ad_sets DROP CONSTRAINT "FK_ad_sets_ad_campaigns_AdCampaignId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260623151611_RemoveAdSetShadowForeignKey') THEN
    DROP INDEX "IX_content_calendar_content_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260623151611_RemoveAdSetShadowForeignKey') THEN
    DROP INDEX "IX_ad_sets_AdCampaignId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260623151611_RemoveAdSetShadowForeignKey') THEN
    ALTER TABLE ad_sets DROP COLUMN "AdCampaignId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260623151611_RemoveAdSetShadowForeignKey') THEN
    WITH ranked AS (
        SELECT
            id,
            ROW_NUMBER() OVER (
                PARTITION BY content_id
                ORDER BY updated_at DESC, created_at DESC, id DESC
            ) AS rn
        FROM content_calendar
        WHERE status IN (0, 1)
    )
    UPDATE content_calendar AS cc
    SET
        status = 3,
        last_error = COALESCE(cc.last_error, 'Superseded by a newer active schedule during migration.'),
        updated_at = NOW()
    FROM ranked
    WHERE cc.id = ranked.id
      AND ranked.rn > 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260623151611_RemoveAdSetShadowForeignKey') THEN
    CREATE UNIQUE INDEX "IX_content_calendar_content_id" ON content_calendar (content_id) WHERE "status" IN (0, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260623151611_RemoveAdSetShadowForeignKey') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260623151611_RemoveAdSetShadowForeignKey', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624080916_EnforcePaidBusinessWorkspaceCreation') THEN
    ALTER TABLE payments ALTER COLUMN workspace_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624080916_EnforcePaidBusinessWorkspaceCreation') THEN
    ALTER TABLE payments ADD pending_workspace_name character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624080916_EnforcePaidBusinessWorkspaceCreation') THEN
    ALTER TABLE payments ADD requested_plan integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624080916_EnforcePaidBusinessWorkspaceCreation') THEN
    UPDATE subscriptions AS subscription
    SET is_active = FALSE,
        end_date = COALESCE(subscription.end_date, CURRENT_DATE),
        updated_at = NOW()
    FROM workspaces AS workspace
    WHERE subscription.workspace_id = workspace.id
      AND workspace.workspace_type = 2
      AND subscription.is_active = TRUE
      AND NOT EXISTS (
          SELECT 1
          FROM payments AS payment
          WHERE payment.status = 1
            AND payment.payment_type = 1
            AND (payment.subscription_id = subscription.id OR
                 (payment.subscription_id IS NULL AND payment.workspace_id = workspace.id))
      );

    UPDATE workspaces AS workspace
    SET status = 2,
        subscription_expired_at = NOW() - INTERVAL '1 second',
        archived_at = NULL,
        member_limit = 1,
        updated_at = NOW()
    WHERE workspace.workspace_type = 2
      AND workspace.status <> 5
      AND NOT EXISTS (
          SELECT 1
          FROM payments AS payment
          WHERE payment.workspace_id = workspace.id
            AND payment.status = 1
            AND payment.payment_type = 1
      );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624080916_EnforcePaidBusinessWorkspaceCreation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260624080916_EnforcePaidBusinessWorkspaceCreation', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624090000_NormalizeUnpaidBusinessWorkspaces') THEN
    UPDATE subscriptions AS subscription
    SET is_active = FALSE,
        end_date = COALESCE(subscription.end_date, CURRENT_DATE),
        updated_at = NOW()
    FROM workspaces AS workspace
    WHERE subscription.workspace_id = workspace.id
      AND workspace.workspace_type = 2
      AND subscription.is_active = TRUE
      AND NOT EXISTS (
          SELECT 1
          FROM payments AS payment
          WHERE payment.status = 1
            AND payment.payment_type = 1
            AND (payment.subscription_id = subscription.id OR
                 (payment.subscription_id IS NULL AND payment.workspace_id = workspace.id))
      );

    UPDATE workspaces AS workspace
    SET status = 2,
        subscription_expired_at = NOW() - INTERVAL '1 second',
        archived_at = NULL,
        member_limit = 1,
        updated_at = NOW()
    WHERE workspace.workspace_type = 2
      AND workspace.status <> 5
      AND NOT EXISTS (
          SELECT 1
          FROM payments AS payment
          WHERE payment.workspace_id = workspace.id
            AND payment.status = 1
            AND payment.payment_type = 1
      );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624090000_NormalizeUnpaidBusinessWorkspaces') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260624090000_NormalizeUnpaidBusinessWorkspaces', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624171408_AddContentIsAiGenerated') THEN
    ALTER TABLE contents ADD is_ai_generated boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260624171408_AddContentIsAiGenerated') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260624171408_AddContentIsAiGenerated', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629134130_AddMediaProviderTracking') THEN
    ALTER TABLE ai_generations ADD provider_name character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629134130_AddMediaProviderTracking') THEN
    ALTER TABLE ai_generations ADD video_job_id character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629134130_AddMediaProviderTracking') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260629134130_AddMediaProviderTracking', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629174021_AddDeploymentStatus') THEN
    ALTER TABLE contents
    ADD COLUMN IF NOT EXISTS tags jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629174021_AddDeploymentStatus') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS deployment_status integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629174021_AddDeploymentStatus') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS deployment_step integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629174021_AddDeploymentStatus') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260629174021_AddDeploymentStatus', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS content_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS product_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    CREATE INDEX IF NOT EXISTS "IX_ad_campaigns_content_id"
    ON ad_campaigns (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    CREATE INDEX IF NOT EXISTS "IX_ad_campaigns_product_id"
    ON ad_campaigns (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    DO $$
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM pg_constraint
            WHERE conname = 'FK_ad_campaigns_contents_content_id'
        ) THEN
            ALTER TABLE ad_campaigns
            ADD CONSTRAINT "FK_ad_campaigns_contents_content_id"
            FOREIGN KEY (content_id) REFERENCES contents(id);
        END IF;
    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    DO $$
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM pg_constraint
            WHERE conname = 'FK_ad_campaigns_products_product_id'
        ) THEN
            ALTER TABLE ad_campaigns
            ADD CONSTRAINT "FK_ad_campaigns_products_product_id"
            FOREIGN KEY (product_id) REFERENCES products(id);
        END IF;
    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260629191750_AddProductAndContentToCampaign', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629193645_AddTargetingToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS targeting jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629193645_AddTargetingToCampaign') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260629193645_AddTargetingToCampaign', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629195735_AddInsightsFieldsToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS clicks bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629195735_AddInsightsFieldsToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS conversions bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629195735_AddInsightsFieldsToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS impressions bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629195735_AddInsightsFieldsToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS spend numeric(12,2) NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629195735_AddInsightsFieldsToCampaign') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260629195735_AddInsightsFieldsToCampaign', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260630112752_AddLandingUrlToCampaign') THEN
    ALTER TABLE ad_campaigns
    ADD COLUMN IF NOT EXISTS landing_url character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260630112752_AddLandingUrlToCampaign') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260630112752_AddLandingUrlToCampaign', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260630124621_EnsureAllWorkspacesHaveActiveSubscription') THEN
    -- 1. Backfill workspace_id for subscriptions still missing it
    --    Join through profile → user → owned workspace_member
    UPDATE subscriptions sub
    SET workspace_id = map.workspace_id,
        updated_at = NOW()
    FROM (
        SELECT DISTINCT ON (p.id)
            p.id AS profile_id,
            wm.workspace_id
        FROM profiles p
        JOIN workspace_members wm ON wm.user_id = p.user_id
        WHERE wm.is_active = TRUE
          AND wm.role = 1
          AND wm.workspace_id IS NOT NULL
        ORDER BY p.id, wm.joined_at ASC
    ) map
    WHERE sub.profile_id = map.profile_id
      AND sub.workspace_id IS NULL;

    -- 2. Create Free subscriptions for any workspace that lacks one
    INSERT INTO subscriptions (
        id, profile_id, workspace_id, plan,
        quota_posts_per_month, quota_ai_content_per_day, quota_ai_images_per_day,
        quota_platforms, quota_accounts, analysis_level,
        quota_ad_budget_monthly, quota_ad_campaigns,
        start_date, end_date, is_active, is_deleted, created_at, updated_at)
    SELECT
        md5('aisam-ensure-subscription:' || w.id::text)::uuid,
        NULL,
        w.id,
        0,
        20, 0, 0, 1, 1, 0, 0, 0,
        CURRENT_DATE,
        NULL,
        TRUE,
        FALSE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    FROM workspaces w
    WHERE w.status = 1
      AND NOT EXISTS (
          SELECT 1 FROM subscriptions s
          WHERE s.workspace_id = w.id
            AND s.is_active = TRUE
            AND s.is_deleted = FALSE
      );

    -- 3. Create credit wallets for workspaces still missing one
    INSERT INTO credit_wallets (id, workspace_id, balance, created_at, updated_at)
    SELECT
        md5('aisam-ensure-wallet:' || w.id::text)::uuid,
        w.id,
        0,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    FROM workspaces w
    WHERE NOT EXISTS (
        SELECT 1 FROM credit_wallets cw WHERE cw.workspace_id = w.id
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260630124621_EnsureAllWorkspacesHaveActiveSubscription') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260630124621_EnsureAllWorkspacesHaveActiveSubscription', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260704131900_AllowMultiPlatformActiveSchedules') THEN
    DROP INDEX "IX_content_calendar_content_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260704131900_AllowMultiPlatformActiveSchedules') THEN
    CREATE UNIQUE INDEX "IX_content_calendar_content_id_integration_id" ON content_calendar (content_id, integration_id) WHERE "status" IN (0, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260704131900_AllowMultiPlatformActiveSchedules') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260704131900_AllowMultiPlatformActiveSchedules', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260705182503_AddSystemSettings') THEN
    CREATE TABLE system_settings (
        id uuid NOT NULL,
        key character varying(100) NOT NULL,
        value jsonb NOT NULL,
        description character varying(500),
        updated_by uuid,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_system_settings" PRIMARY KEY (id),
        CONSTRAINT "FK_system_settings_users_updated_by" FOREIGN KEY (updated_by) REFERENCES users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260705182503_AddSystemSettings') THEN
    CREATE UNIQUE INDEX "IX_system_settings_key" ON system_settings (key);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260705182503_AddSystemSettings') THEN
    CREATE INDEX "IX_system_settings_updated_by" ON system_settings (updated_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260705182503_AddSystemSettings') THEN

                    INSERT INTO users (id, email, full_name, role, is_email_verified, password_hash, password_salt, created_at)
                    SELECT gen_random_uuid(), 'admin@aisam.com', 'Super Admin', 2, true, 'ezbsYCnaHQFB3i2hTxLMyriAWmWFpfljIiYjz6bTjInYp/tbJd+5yX6UYEpHBDIoDPl6PZQSKFd+0iN5LCmipA==', 'ogj9QceE0qO+BFbltp3UHXSIDc56ZyL+YGuDXWIrMISPmhjiqrkE6SKdqgGXTGQLl2jVfLAmILxIlhGbesgl1F1Og7dVJ1RjjIVrmdWSey8/c39agLKPJ/UGIEYliPs+fSCD3NS3OyATO/rB6EVNwOzkUyWnTzgmKhUxR/CnN2E=', NOW()
                    WHERE NOT EXISTS (SELECT 1 FROM users WHERE email = 'admin@aisam.com');
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260705182503_AddSystemSettings') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260705182503_AddSystemSettings', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706013210_AddVideoGenerationJobs') THEN
    CREATE TABLE video_generation_jobs (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        user_id uuid NOT NULL,
        original_prompt text NOT NULL,
        provider character varying(100) NOT NULL,
        is_fallback boolean NOT NULL,
        status integer NOT NULL DEFAULT 0,
        external_job_id character varying(255),
        segments_count integer,
        current_segment integer,
        video_url character varying(500),
        error_message text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        completed_at timestamp with time zone,
        CONSTRAINT "PK_video_generation_jobs" PRIMARY KEY (id),
        CONSTRAINT "FK_video_generation_jobs_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
        CONSTRAINT "FK_video_generation_jobs_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706013210_AddVideoGenerationJobs') THEN
    CREATE INDEX "IX_video_generation_jobs_is_fallback" ON video_generation_jobs (is_fallback);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706013210_AddVideoGenerationJobs') THEN
    CREATE INDEX "IX_video_generation_jobs_status" ON video_generation_jobs (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706013210_AddVideoGenerationJobs') THEN
    CREATE INDEX "IX_video_generation_jobs_user_id" ON video_generation_jobs (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706013210_AddVideoGenerationJobs') THEN
    CREATE INDEX "IX_video_generation_jobs_workspace_id" ON video_generation_jobs (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706013210_AddVideoGenerationJobs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260706013210_AddVideoGenerationJobs', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE TABLE automation_plans (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        profile_id uuid NOT NULL,
        name character varying(200) NOT NULL,
        source_file_name character varying(255),
        timezone character varying(80) NOT NULL,
        status integer NOT NULL,
        total_items integer NOT NULL,
        valid_items integer NOT NULL,
        failed_items integer NOT NULL,
        estimated_credits integer NOT NULL,
        reserved_credits integer NOT NULL,
        used_credits integer NOT NULL,
        released_credits integer NOT NULL,
        is_deleted boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        confirmed_at timestamp with time zone,
        CONSTRAINT "PK_automation_plans" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE TABLE automation_items (
        id uuid NOT NULL,
        automation_plan_id uuid NOT NULL,
        row_index integer NOT NULL,
        platform character varying(30) NOT NULL,
        idempotency_key character varying(64) NOT NULL,
        brand_id uuid NOT NULL,
        product_id uuid,
        content_id uuid,
        topic character varying(300) NOT NULL,
        objective character varying(100),
        requested_content_type integer NOT NULL,
        tone character varying(100),
        cta character varying(300),
        notes text,
        scheduled_at timestamp with time zone NOT NULL,
        status integer NOT NULL,
        estimated_credits integer NOT NULL,
        used_credits integer NOT NULL,
        validation_errors jsonb,
        source_json jsonb NOT NULL,
        generation_attempt_count integer NOT NULL,
        last_error text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_automation_items" PRIMARY KEY (id),
        CONSTRAINT "FK_automation_items_automation_plans_automation_plan_id" FOREIGN KEY (automation_plan_id) REFERENCES automation_plans (id) ON DELETE CASCADE,
        CONSTRAINT "FK_automation_items_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_automation_items_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE SET NULL,
        CONSTRAINT "FK_automation_items_products_product_id" FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE UNIQUE INDEX "IX_automation_items_automation_plan_id_row_index_platform" ON automation_items (automation_plan_id, row_index, platform);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE INDEX "IX_automation_items_brand_id" ON automation_items (brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE INDEX "IX_automation_items_content_id" ON automation_items (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE UNIQUE INDEX "IX_automation_items_idempotency_key" ON automation_items (idempotency_key);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE INDEX "IX_automation_items_product_id" ON automation_items (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE INDEX "IX_automation_items_scheduled_at" ON automation_items (scheduled_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE INDEX "IX_automation_items_status" ON automation_items (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE INDEX "IX_automation_plans_workspace_id" ON automation_plans (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    CREATE INDEX "IX_automation_plans_workspace_id_created_at" ON automation_plans (workspace_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706081940_AddAutomationPlans') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260706081940_AddAutomationPlans', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706095014_FixAdminPasswordHash') THEN

                    UPDATE users
                    SET password_hash = 'UerdQ3VtiYZ4QTm6hRz+eOmid9LnWaURY30Rxe7vVwDQT07ZVvPYFNfFc86F00bEMnxuaZ6wO9hNxLuiLWvVag==',
                        password_salt = '0YzN6SLaBxlvEmaum9P7ct2gISTgBFv+Iyc8zutGzQKn0lbvJi9D0oH39mwVloTQ0R94qhCKVaarTgAz302y0rlUGrc3A1Q//Q2VEsbQ8I1//pbbWClzhaNQ5rO9bes/uJJ/zX66xrlGfTPaAJJFZByiSXnj5x6XVBA4heUJkJY='
                    WHERE email = 'admin@aisam.com';
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260706095014_FixAdminPasswordHash') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260706095014_FixAdminPasswordHash', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707020157_AddAutomationVideoAndScheduleLinks') THEN
    ALTER TABLE automation_items ADD content_calendar_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707020157_AddAutomationVideoAndScheduleLinks') THEN
    ALTER TABLE automation_items ADD video_job_id character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707020157_AddAutomationVideoAndScheduleLinks') THEN
    ALTER TABLE automation_items ADD video_provider character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707020157_AddAutomationVideoAndScheduleLinks') THEN
    CREATE UNIQUE INDEX "IX_automation_items_content_calendar_id" ON automation_items (content_calendar_id) WHERE content_calendar_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707020157_AddAutomationVideoAndScheduleLinks') THEN
    ALTER TABLE automation_items ADD CONSTRAINT "FK_automation_items_content_calendar_content_calendar_id" FOREIGN KEY (content_calendar_id) REFERENCES content_calendar (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707020157_AddAutomationVideoAndScheduleLinks') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260707020157_AddAutomationVideoAndScheduleLinks', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707021542_AddAutomationOperations') THEN
    ALTER TABLE automation_plans ADD auto_approve boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707021542_AddAutomationOperations') THEN
    ALTER TABLE automation_plans ADD template_source_plan_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707021542_AddAutomationOperations') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260707021542_AddAutomationOperations', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707022201_AddReservedCreditBalance') THEN
    ALTER TABLE credit_wallets ADD reserved_balance bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260707022201_AddReservedCreditBalance') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260707022201_AddReservedCreditBalance', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260708143019_AddCampaignPlatform') THEN
    ALTER TABLE ad_campaigns ADD platform character varying(20) NOT NULL DEFAULT 'facebook';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260708143019_AddCampaignPlatform') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260708143019_AddCampaignPlatform', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713075334_AddSocialIntegrationTargetMetadata') THEN
    ALTER TABLE social_integrations ADD profile_picture_url character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713075334_AddSocialIntegrationTargetMetadata') THEN
    ALTER TABLE social_integrations ADD target_category character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713075334_AddSocialIntegrationTargetMetadata') THEN
    ALTER TABLE social_integrations ADD target_name character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713075334_AddSocialIntegrationTargetMetadata') THEN
    ALTER TABLE social_integrations ADD target_type character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713075334_AddSocialIntegrationTargetMetadata') THEN
    CREATE UNIQUE INDEX "IX_social_integrations_workspace_id_platform_external_id" ON social_integrations (workspace_id, platform, external_id) WHERE "is_deleted" = FALSE AND "external_id" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713075334_AddSocialIntegrationTargetMetadata') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260713075334_AddSocialIntegrationTargetMetadata', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713100840_AddProductKnowledgeProfile') THEN
    ALTER TABLE products ADD category character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713100840_AddProductKnowledgeProfile') THEN
    ALTER TABLE products ADD knowledge_profile text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713100840_AddProductKnowledgeProfile') THEN
    ALTER TABLE products ADD primary_use text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713100840_AddProductKnowledgeProfile') THEN
    ALTER TABLE products ADD target_audience text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713100840_AddProductKnowledgeProfile') THEN
    ALTER TABLE products ADD usp text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713100840_AddProductKnowledgeProfile') THEN
    ALTER TABLE products ADD visual_identity text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260713100840_AddProductKnowledgeProfile') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260713100840_AddProductKnowledgeProfile', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260715064839_RepairMultiPlatformScheduleIndex') THEN
    DROP INDEX IF EXISTS "IX_content_calendar_content_id";
    DROP INDEX IF EXISTS ix_content_calendar_content_id;

    CREATE UNIQUE INDEX IF NOT EXISTS "IX_content_calendar_content_id_integration_id"
    ON content_calendar (content_id, integration_id)
    WHERE "status" IN (0, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260715064839_RepairMultiPlatformScheduleIndex') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260715064839_RepairMultiPlatformScheduleIndex', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724220222_AddContentThumbnailUrl') THEN
    ALTER TABLE contents ADD thumbnail_url character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724220222_AddContentThumbnailUrl') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260724220222_AddContentThumbnailUrl', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260725090000_AddBrandIsDeleted') THEN
    ALTER TABLE brands
    ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260725090000_AddBrandIsDeleted') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260725090000_AddBrandIsDeleted', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260728185924_AddCampaignStatusColumn') THEN
    ALTER TABLE ad_campaigns ADD status integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260728185924_AddCampaignStatusColumn') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260728185924_AddCampaignStatusColumn', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260801162958_AddAdAccountCurrencyToCampaign') THEN
    ALTER TABLE ad_campaigns ADD ad_account_currency character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260801162958_AddAdAccountCurrencyToCampaign') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260801162958_AddAdAccountCurrencyToCampaign', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260802120133_AddCampaignDeploymentMessage') THEN
    ALTER TABLE ad_campaigns ADD deployment_message character varying(2000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260802120133_AddCampaignDeploymentMessage') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260802120133_AddCampaignDeploymentMessage', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260802121500_EnsureCampaignDeploymentMessageColumn') THEN
    ALTER TABLE ad_campaigns ADD COLUMN IF NOT EXISTS deployment_message character varying(2000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260802121500_EnsureCampaignDeploymentMessageColumn') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260802121500_EnsureCampaignDeploymentMessageColumn', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260805090000_EnsureMultiAccountScheduleIndex') THEN
    DROP INDEX IF EXISTS "IX_content_calendar_content_id";
    DROP INDEX IF EXISTS ix_content_calendar_content_id;

    CREATE UNIQUE INDEX IF NOT EXISTS "IX_content_calendar_content_id_integration_id"
    ON content_calendar (content_id, integration_id)
    WHERE "status" IN (0, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260805090000_EnsureMultiAccountScheduleIndex') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260805090000_EnsureMultiAccountScheduleIndex', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806101606_AddUserSuspensionState') THEN
    ALTER TABLE users ADD is_active boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806101606_AddUserSuspensionState') THEN
    ALTER TABLE users ADD suspended_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806101606_AddUserSuspensionState') THEN
    ALTER TABLE users ADD suspended_by uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806101606_AddUserSuspensionState') THEN
    ALTER TABLE users ADD suspension_reason character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806101606_AddUserSuspensionState') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260806101606_AddUserSuspensionState', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807023054_AddRefundFieldsToPayment2') THEN
    ALTER TABLE payments ADD refund_reason character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807023054_AddRefundFieldsToPayment2') THEN
    ALTER TABLE payments ADD refunded_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807023054_AddRefundFieldsToPayment2') THEN
    ALTER TABLE payments ADD refunded_by uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807023054_AddRefundFieldsToPayment2') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260807023054_AddRefundFieldsToPayment2', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807023322_AddContentPlatformRejection') THEN
    ALTER TABLE contents ADD platform_rejection_reason character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807023322_AddContentPlatformRejection') THEN
    ALTER TABLE contents ADD rejected_platform character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807023322_AddContentPlatformRejection') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260807023322_AddContentPlatformRejection', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810091222_IncreaseProductPricePrecision') THEN
    ALTER TABLE products ALTER COLUMN price TYPE numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810091222_IncreaseProductPricePrecision') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260810091222_IncreaseProductPricePrecision', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260814060224_AddApproverUserIdToApprovalsManually') THEN
    ALTER TABLE approvals ADD COLUMN IF NOT EXISTS approver_user_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260814060224_AddApproverUserIdToApprovalsManually') THEN
    CREATE INDEX IF NOT EXISTS "IX_approvals_approver_user_id" ON approvals (approver_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260814060224_AddApproverUserIdToApprovalsManually') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260814060224_AddApproverUserIdToApprovalsManually', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260814062631_MakeApproverUserIdNullable') THEN
    ALTER TABLE approvals DROP CONSTRAINT "FK_approvals_users_approver_user_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260814062631_MakeApproverUserIdNullable') THEN
    ALTER TABLE approvals ALTER COLUMN approver_user_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260814062631_MakeApproverUserIdNullable') THEN
    ALTER TABLE approvals ADD CONSTRAINT "FK_approvals_users_approver_user_id" FOREIGN KEY (approver_user_id) REFERENCES users (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260814062631_MakeApproverUserIdNullable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260814062631_MakeApproverUserIdNullable', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260818064203_AddReachToPerformanceReports') THEN
    ALTER TABLE performance_reports ADD reach bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260818064203_AddReachToPerformanceReports') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260818064203_AddReachToPerformanceReports', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260818070730_AddClicksToPerformanceReports') THEN
    ALTER TABLE performance_reports ADD clicks bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260818070730_AddClicksToPerformanceReports') THEN

                    UPDATE performance_reports
                    SET clicks = GREATEST(
                        COALESCE((raw_data::jsonb ->> 'clicks')::bigint, 0),
                        COALESCE((raw_data::jsonb ->> 'trackedClicks')::bigint, 0)
                    )
                    WHERE raw_data IS NOT NULL AND raw_data::text <> '' AND raw_data::text <> '{}';
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260818070730_AddClicksToPerformanceReports') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260818070730_AddClicksToPerformanceReports', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260818120201_AddPatternIdToAiGeneration') THEN
    ALTER TABLE ai_generations ADD pattern_id character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260818120201_AddPatternIdToAiGeneration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260818120201_AddPatternIdToAiGeneration', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819024030_AddHolidayEventsAndContentSource') THEN
    ALTER TABLE contents ADD generated_source character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819024030_AddHolidayEventsAndContentSource') THEN
    CREATE TABLE holiday_events (
        id uuid NOT NULL,
        name character varying(255) NOT NULL,
        local_name character varying(255),
        exact_date timestamp with time zone NOT NULL,
        year integer NOT NULL,
        country_code character varying(10) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_holiday_events" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819024030_AddHolidayEventsAndContentSource') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260819024030_AddHolidayEventsAndContentSource', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819044656_AddHolidayEventFlags') THEN
    ALTER TABLE holiday_events ADD is_active boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819044656_AddHolidayEventFlags') THEN
    ALTER TABLE holiday_events ADD is_manually_overridden boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819044656_AddHolidayEventFlags') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260819044656_AddHolidayEventFlags', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819114225_SyncMissingSchemaFromDb') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260819114225_SyncMissingSchemaFromDb', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260820013404_Wave3Fixes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260820013404_Wave3Fixes', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260820094328_MakeAutomationItemBrandIdNullable') THEN
    ALTER TABLE automation_items ALTER COLUMN brand_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260820094328_MakeAutomationItemBrandIdNullable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260820094328_MakeAutomationItemBrandIdNullable', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821024050_AddProductAdditionalFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260821024050_AddProductAdditionalFields', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    DO $$ BEGIN
        IF EXISTS (SELECT 1 FROM teams) THEN
            RAISE EXCEPTION 'PermissionAccessControl requires a reviewed legacy Team-to-Workspace mapping; no changes applied.';
        END IF;
    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE teams ALTER COLUMN profile_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE teams ADD workspace_id uuid NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE team_brands ADD channel_access_mode integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE credit_usage_records ADD balance_after bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE credit_usage_records ADD balance_before bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE credit_usage_records ADD brand_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE credit_usage_records ADD integration_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE credit_usage_records ADD reference_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE credit_usage_records ADD team_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE contents ADD primary_creator_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE audit_logs ADD workspace_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE TABLE collaboration_tasks (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        team_id uuid NOT NULL,
        content_id uuid NOT NULL,
        integration_id uuid,
        assignee_id uuid NOT NULL,
        assigned_by uuid NOT NULL,
        title character varying(255) NOT NULL,
        status integer NOT NULL,
        blocked_reason character varying(64),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_collaboration_tasks" PRIMARY KEY (id),
        CONSTRAINT "FK_collaboration_tasks_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_collaboration_tasks_teams_team_id" FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE TABLE content_participations (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        content_id uuid NOT NULL,
        user_id uuid NOT NULL,
        recorded_by uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_content_participations" PRIMARY KEY (id),
        CONSTRAINT "FK_content_participations_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE TABLE team_channel_access (
        id uuid NOT NULL,
        team_brand_id uuid NOT NULL,
        integration_id uuid NOT NULL,
        CONSTRAINT "PK_team_channel_access" PRIMARY KEY (id),
        CONSTRAINT "FK_team_channel_access_social_integrations_integration_id" FOREIGN KEY (integration_id) REFERENCES social_integrations (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_team_channel_access_team_brands_team_brand_id" FOREIGN KEY (team_brand_id) REFERENCES team_brands (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE TABLE temporary_access_grants (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        task_id uuid NOT NULL,
        user_id uuid NOT NULL,
        granted_by uuid NOT NULL,
        granted_at timestamp with time zone NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        revoked_at timestamp with time zone,
        reason character varying(1000) NOT NULL,
        can_edit boolean NOT NULL,
        CONSTRAINT "PK_temporary_access_grants" PRIMARY KEY (id),
        CONSTRAINT "FK_temporary_access_grants_collaboration_tasks_task_id" FOREIGN KEY (task_id) REFERENCES collaboration_tasks (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_teams_workspace_id" ON teams (workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE UNIQUE INDEX "IX_team_members_team_id_user_id" ON team_members (team_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE UNIQUE INDEX "IX_team_brands_team_id_brand_id" ON team_brands (team_id, brand_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_credit_usage_records_workspace_id_team_id_created_at" ON credit_usage_records (workspace_id, team_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_contents_workspace_id_primary_creator_id" ON contents (workspace_id, primary_creator_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_collaboration_tasks_content_id" ON collaboration_tasks (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_collaboration_tasks_team_id" ON collaboration_tasks (team_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_collaboration_tasks_workspace_id_assignee_id_status" ON collaboration_tasks (workspace_id, assignee_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE UNIQUE INDEX "IX_content_participations_content_id_user_id" ON content_participations (content_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_team_channel_access_integration_id" ON team_channel_access (integration_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE UNIQUE INDEX "IX_team_channel_access_team_brand_id_integration_id" ON team_channel_access (team_brand_id, integration_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_temporary_access_grants_task_id" ON temporary_access_grants (task_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    CREATE INDEX "IX_temporary_access_grants_user_id_expires_at" ON temporary_access_grants (user_id, expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    ALTER TABLE teams ADD CONSTRAINT "FK_teams_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    DO $$ BEGIN
        IF EXISTS (SELECT 1 FROM workspace_members GROUP BY workspace_id, user_id HAVING COUNT(*) > 1) THEN
            RAISE EXCEPTION 'Duplicate workspace membership requires review before default Team creation.';
        END IF;
    END $$;
    INSERT INTO teams (id, workspace_id, profile_id, name, is_deleted, status, created_at)
    SELECT gen_random_uuid(), w.id, NULL, w.name, false, 0, CURRENT_TIMESTAMP
    FROM workspaces w WHERE NOT EXISTS (SELECT 1 FROM teams t WHERE t.workspace_id = w.id);
    INSERT INTO team_members (id, team_id, user_id, role, permissions, joined_at, is_active)
    SELECT gen_random_uuid(), t.id, m.user_id,
        CASE m.role WHEN 1 THEN 'Owner' WHEN 2 THEN 'Manager' WHEN 3 THEN 'ContentCreator' ELSE 'Viewer' END,
        '[]'::jsonb, CURRENT_TIMESTAMP, m.is_active
    FROM teams t JOIN workspace_members m ON m.workspace_id = t.workspace_id
    WHERE NOT EXISTS (SELECT 1 FROM team_members tm WHERE tm.team_id = t.id AND tm.user_id = m.user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904083332_PermissionAccessControl') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904083332_PermissionAccessControl', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904092723_CampaignChannelAttribution') THEN
    ALTER TABLE ad_campaigns ADD integration_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904092723_CampaignChannelAttribution') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904092723_CampaignChannelAttribution', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    DROP INDEX "IX_temporary_access_grants_task_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    DROP INDEX "IX_collaboration_tasks_team_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE audit_logs ADD affected_user_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE audit_logs ADD approved_by uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE audit_logs ADD executed_by_system boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE audit_logs ADD reference_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE audit_logs ADD requested_by uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE audit_logs ADD team_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE teams ADD CONSTRAINT "AK_teams_id_workspace_id" UNIQUE (id, workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE collaboration_tasks ADD CONSTRAINT "AK_collaboration_tasks_id_workspace_id" UNIQUE (id, workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE TABLE execution_operations (
        id uuid NOT NULL,
        workspace_id uuid NOT NULL,
        actor_user_id uuid NOT NULL,
        team_id uuid,
        resource_id uuid NOT NULL,
        resource_type character varying(50) NOT NULL,
        brand_id uuid,
        integration_id uuid,
        requested_action character varying(50) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        reference_id uuid NOT NULL,
        approval_authority character varying(50),
        approved_by uuid,
        approved_at timestamp with time zone,
        execution_policy character varying(80) NOT NULL,
        policy_version integer NOT NULL,
        enqueue_authorized_at timestamp with time zone,
        CONSTRAINT "PK_execution_operations" PRIMARY KEY (id),
        CONSTRAINT "FK_execution_operations_teams_team_id_workspace_id" FOREIGN KEY (team_id, workspace_id) REFERENCES teams (id, workspace_id) ON DELETE RESTRICT,
        CONSTRAINT "FK_execution_operations_users_actor_user_id" FOREIGN KEY (actor_user_id) REFERENCES users (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_execution_operations_workspaces_workspace_id" FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE INDEX "IX_temporary_access_grants_task_id_workspace_id" ON temporary_access_grants (task_id, workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE UNIQUE INDEX "IX_contents_id_workspace_id" ON contents (id, workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE INDEX "IX_collaboration_tasks_team_id_workspace_id" ON collaboration_tasks (team_id, workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE INDEX "IX_execution_operations_actor_user_id" ON execution_operations (actor_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE UNIQUE INDEX "IX_execution_operations_resource_type_reference_id_requested_a~" ON execution_operations (resource_type, reference_id, requested_action);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE INDEX "IX_execution_operations_team_id_workspace_id" ON execution_operations (team_id, workspace_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    CREATE INDEX "IX_execution_operations_workspace_id_team_id_created_at" ON execution_operations (workspace_id, team_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE collaboration_tasks ADD CONSTRAINT "FK_collaboration_tasks_teams_team_id_workspace_id" FOREIGN KEY (team_id, workspace_id) REFERENCES teams (id, workspace_id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE temporary_access_grants ADD CONSTRAINT "FK_temporary_access_grants_collaboration_tasks_task_id_workspa~" FOREIGN KEY (task_id, workspace_id) REFERENCES collaboration_tasks (id, workspace_id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    ALTER TABLE collaboration_tasks ADD CONSTRAINT fk_collaboration_content_workspace
      FOREIGN KEY (content_id, workspace_id) REFERENCES contents (id, workspace_id) ON DELETE RESTRICT;
    ALTER TABLE content_participations ADD CONSTRAINT fk_participation_content_workspace
      FOREIGN KEY (content_id, workspace_id) REFERENCES contents (id, workspace_id) ON DELETE RESTRICT;
    ALTER TABLE temporary_access_grants ADD CONSTRAINT ck_temporary_grant_dates
      CHECK (expires_at > granted_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904103845_ExecutionAttributionAndIntegrity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904103845_ExecutionAttributionAndIntegrity', '9.0.9');
    END IF;
END $EF$;
COMMIT;

