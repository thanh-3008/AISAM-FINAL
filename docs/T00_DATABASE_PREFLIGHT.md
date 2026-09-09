# T00 — Database preflight

Generated UTC: 2026-09-07T05:16:47.3004232Z

Read-only repeatable-read transaction. Aggregate counts and schema metadata only; no database mutations.

## Table counts

| Metric / field | Value / definition |
|---|---|
| teams | 61 |
| team_brands | 36 |
| team_members | 66 |
| contents | 542 |
| posts | 179 |
| assets | 0 |
| schedules | 139 |
| integrations | 147 |

## Migration risks

| Metric / field | Value / definition |
|---|---|
| teams_without_workspace_via_legacy_profile | 61 |
| cross_workspace_team_brand | 0 |
| duplicate_team_brand_pairs | 0 |
| duplicate_team_member_pairs | 0 |
| content_brand_workspace_mismatch | 0 |
| post_channel_brand_mismatch | 1 |
| legacy_contents_with_video | 59 |
| legacy_contents_with_images | 271 |
| assets_without_uploader | 0 |

## Current database Team workspace (schema drift check)

| Metric / field | Value / definition |
|---|---|
| teams_invalid_direct_workspace | 0 |
| team_brand_direct_workspace_mismatch | 0 |
| team_profile_missing | 61 |
| contents_without_primary_creator | 542 |
| contents_with_video_urls | 59 |

## Attribution and media schema

| Metric / field | Value / definition |
|---|---|
| assets.uploaded_by | uuid, nullable=YES |
| assets.metadata | jsonb, nullable=YES |
| contents.profile_id | uuid, nullable=NO |
| contents.image_url | jsonb, nullable=YES |
| contents.video_url | character varying, nullable=YES |
| contents.workspace_id | uuid, nullable=NO |
| contents.primary_creator_id | uuid, nullable=YES |
| contents.video_urls | jsonb, nullable=YES |
| teams.profile_id | uuid, nullable=YES |
| teams.workspace_id | uuid, nullable=NO |

## Assignment indexes

| Metric / field | Value / definition |
|---|---|
| team_brands | CREATE INDEX "IX_team_brands_brand_id" ON public.team_brands USING btree (brand_id) |
| team_brands | CREATE INDEX "IX_team_brands_is_active" ON public.team_brands USING btree (is_active) |
| team_brands | CREATE INDEX "IX_team_brands_team_id" ON public.team_brands USING btree (team_id) |
| team_brands | CREATE UNIQUE INDEX "IX_team_brands_team_id_brand_id" ON public.team_brands USING btree (team_id, brand_id) |
| team_brands | CREATE UNIQUE INDEX "PK_team_brands" ON public.team_brands USING btree (id) |
| team_members | CREATE INDEX "IX_team_members_team_id" ON public.team_members USING btree (team_id) |
| team_members | CREATE UNIQUE INDEX "IX_team_members_team_id_user_id" ON public.team_members USING btree (team_id, user_id) |
| team_members | CREATE INDEX "IX_team_members_user_id" ON public.team_members USING btree (user_id) |
| team_members | CREATE UNIQUE INDEX "PK_team_members" ON public.team_members USING btree (id) |

## Applied migrations

| Metric / field | Value / definition |
|---|---|
| 20251102025736_Initial | 9.0.9 |
| 20260124120929_AddCustomAuthenticationTables | 9.0.9 |
| 20260124133308_verifytoken | 9.0.9 |
| 20260124135926_UpdatePasswordSaltLength | 9.0.9 |
| 20260127160619_UpdateSubscriptionPayOS | 9.0.9 |
| 20260531161937_RemovePostSocialIntegrationShadowFk | 9.0.9 |
| 20260601095652_AddContentCalendarSchedulingRuntimeFields | 9.0.9 |
| 20260604142029_AddContentCalendarTable | 9.0.9 |
| 20260610064359_AddWorkspaceFoundation | 9.0.9 |
| 20260610160919_AddWorkspaceInvitationFoundation | 9.0.9 |
| 20260610172441_AddWorkspaceMemberLimit | 9.0.9 |
| 20260611085418_EnforceSingleActiveWorkspaceOwner | 9.0.9 |
| 20260611092549_AddWorkspacePaymentSubscriptionOwnership | 9.0.9 |
| 20260611115818_AddCreditWalletAndUsageTracking | 9.0.9 |
| 20260611123701_AddCreditPackPaymentType | 9.0.9 |
| 20260611131708_AddWorkspaceInvitationQuotaModes | 9.0.9 |
| 20260612020911_FixEfModelConfigurationWarnings | 9.0.9 |
| 20260612024207_AddBrandWorkspaceOwnership | 9.0.9 |
| 20260613011615_AddRemainingDomainWorkspaceOwnership | 9.0.9 |
| 20260613020441_BackfillLegacyWorkspaceDataAndLockOwnership | 9.0.9 |
| 20260613130339_ProvisionMissingPersonalFreePlan | 9.0.9 |
| 20260616150207_AddWorkspaceBusinessProfile | 9.0.9 |
| 20260618114250_AddProfileWorkspaceOwnership | 9.0.9 |
| 20260620153457_AddProductStock | 9.0.9 |
| 20260623151611_RemoveAdSetShadowForeignKey | 9.0.9 |
| 20260624080916_EnforcePaidBusinessWorkspaceCreation | 9.0.9 |
| 20260624090000_NormalizeUnpaidBusinessWorkspaces | 9.0.9 |
| 20260624171408_AddContentIsAiGenerated | 9.0.9 |
| 20260629134130_AddMediaProviderTracking | 9.0.9 |
| 20260629174021_AddDeploymentStatus | 9.0.9 |
| 20260629191750_AddProductAndContentToCampaign | 9.0.9 |
| 20260629193645_AddTargetingToCampaign | 9.0.9 |
| 20260629195735_AddInsightsFieldsToCampaign | 9.0.9 |
| 20260630112752_AddLandingUrlToCampaign | 9.0.0 |
| 20260630124621_EnsureAllWorkspacesHaveActiveSubscription | 9.0.9 |
| 20260704131900_AllowMultiPlatformActiveSchedules | 9.0.9 |
| 20260705182503_AddSystemSettings | 9.0.9 |
| 20260706013210_AddVideoGenerationJobs | 9.0.9 |
| 20260706081940_AddAutomationPlans | 9.0.9 |
| 20260706095014_FixAdminPasswordHash | 9.0.9 |
| 20260707020157_AddAutomationVideoAndScheduleLinks | 9.0.9 |
| 20260707021542_AddAutomationOperations | 9.0.9 |
| 20260707022201_AddReservedCreditBalance | 9.0.9 |
| 20260708143019_AddCampaignPlatform | 9.0.9 |
| 20260713075334_AddSocialIntegrationTargetMetadata | 9.0.9 |
| 20260713100840_AddProductKnowledgeProfile | 9.0.9 |
| 20260715064839_RepairMultiPlatformScheduleIndex | 9.0.9 |
| 20260724220222_AddContentThumbnailUrl | 9.0.9 |
| 20260725090000_AddBrandIsDeleted | 9.0.9 |
| 20260727035935_AddCampaignInsightSnapshots | 9.0.9 |
| 20260728185924_AddCampaignStatusColumn | 9.0.9 |
| 20260801161750_AddAdAccountCurrencyToCampaign | 9.0.9 |
| 20260801162958_AddAdAccountCurrencyToCampaign | 9.0.9 |
| 20260802120133_AddCampaignDeploymentMessage | 9.0.9 |
| 20260802121500_EnsureCampaignDeploymentMessageColumn | 9.0.9 |
| 20260805090000_EnsureMultiAccountScheduleIndex | 9.0.9 |
| 20260806101606_AddUserSuspensionState | 9.0.9 |
| 20260807023054_AddRefundFieldsToPayment2 | 9.0.9 |
| 20260807023322_AddContentPlatformRejection | 9.0.9 |
| 20260810091222_IncreaseProductPricePrecision | 9.0.9 |
| 20260814060224_AddApproverUserIdToApprovalsManually | 9.0.9 |
| 20260814062631_MakeApproverUserIdNullable | 9.0.9 |
| 20260815110228_EnhanceBrandKitAndProductCatalog | 9.0.9 |
| 20260818064203_AddReachToPerformanceReports | 9.0.9 |
| 20260818070730_AddClicksToPerformanceReports | 9.0.9 |
| 20260818120201_AddPatternIdToAiGeneration | 9.0.9 |
| 20260819024030_AddHolidayEventsAndContentSource | 9.0.9 |
| 20260819044656_AddHolidayEventFlags | 9.0.9 |
| 20260819114225_SyncMissingSchemaFromDb | 9.0.9 |
| 20260820013404_Wave3Fixes | 9.0.9 |
| 20260820094328_MakeAutomationItemBrandIdNullable | 9.0.9 |
| 20260821024050_AddProductAdditionalFields | 9.0.9 |
| 20260904083332_PermissionAccessControl | 9.0.9 |
| 20260904092723_CampaignChannelAttribution | 9.0.9 |
| 20260904103845_ExecutionAttributionAndIntegrity | 9.0.9 |
| 20260904112639_MutationPermissionRevision | 9.0.9 |
| 20260906041041_ContentOwnershipBoundary | 9.0.9 |
| 20260907080000_FixUserIsActiveDefaultAndBackfill | 9.0.9 |
| 20260907083000_AddContentVideoUrlsJsonb | 8.0.4 |

