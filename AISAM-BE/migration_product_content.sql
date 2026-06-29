START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    ALTER TABLE ad_campaigns ADD content_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    ALTER TABLE ad_campaigns ADD product_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    CREATE INDEX "IX_ad_campaigns_content_id" ON ad_campaigns (content_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    CREATE INDEX "IX_ad_campaigns_product_id" ON ad_campaigns (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    ALTER TABLE ad_campaigns ADD CONSTRAINT "FK_ad_campaigns_contents_content_id" FOREIGN KEY (content_id) REFERENCES contents (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    ALTER TABLE ad_campaigns ADD CONSTRAINT "FK_ad_campaigns_products_product_id" FOREIGN KEY (product_id) REFERENCES products (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260629191750_AddProductAndContentToCampaign') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260629191750_AddProductAndContentToCampaign', '9.0.9');
    END IF;
END $EF$;
COMMIT;

