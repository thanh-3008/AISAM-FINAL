# T00 — Database preflight

Generated UTC: 2026-09-07T16:22:32.2172343Z

Read-only repeatable-read transaction. Aggregate counts and schema metadata only; no database mutations.

EF mapping smoke check: Team, TeamBrand, TeamChannelAccess and Content queries passed.

## Permission schema inventory

| Metric / field | Value / definition |
|---|---|
| audit_logs.id | uuid, nullable=NO |
| audit_logs.actor_id | uuid, nullable=NO |
| audit_logs.action_type | character varying, nullable=NO |
| audit_logs.target_table | character varying, nullable=NO |
| audit_logs.target_id | uuid, nullable=NO |
| audit_logs.old_values | jsonb, nullable=YES |
| audit_logs.new_values | jsonb, nullable=YES |
| audit_logs.notes | text, nullable=YES |
| audit_logs.created_at | timestamp with time zone, nullable=NO |
| audit_logs.workspace_id | uuid, nullable=YES |
| audit_logs.affected_user_id | uuid, nullable=YES |
| audit_logs.approved_by | uuid, nullable=YES |
| audit_logs.executed_by_system | boolean, nullable=NO |
| audit_logs.reference_id | uuid, nullable=YES |
| audit_logs.requested_by | uuid, nullable=YES |
| audit_logs.team_id | uuid, nullable=YES |
| automation_plans.id | uuid, nullable=NO |
| automation_plans.workspace_id | uuid, nullable=NO |
| automation_plans.profile_id | uuid, nullable=NO |
| automation_plans.name | character varying, nullable=NO |
| automation_plans.source_file_name | character varying, nullable=YES |
| automation_plans.timezone | character varying, nullable=NO |
| automation_plans.status | integer, nullable=NO |
| automation_plans.total_items | integer, nullable=NO |
| automation_plans.valid_items | integer, nullable=NO |
| automation_plans.failed_items | integer, nullable=NO |
| automation_plans.estimated_credits | integer, nullable=NO |
| automation_plans.reserved_credits | integer, nullable=NO |
| automation_plans.used_credits | integer, nullable=NO |
| automation_plans.released_credits | integer, nullable=NO |
| automation_plans.is_deleted | boolean, nullable=NO |
| automation_plans.created_at | timestamp with time zone, nullable=NO |
| automation_plans.updated_at | timestamp with time zone, nullable=NO |
| automation_plans.confirmed_at | timestamp with time zone, nullable=YES |
| automation_plans.auto_approve | boolean, nullable=NO |
| automation_plans.template_source_plan_id | uuid, nullable=YES |
| content_calendar.id | uuid, nullable=NO |
| content_calendar.content_id | uuid, nullable=NO |
| content_calendar.scheduled_date | timestamp with time zone, nullable=NO |
| content_calendar.scheduled_time | interval, nullable=YES |
| content_calendar.timezone | character varying, nullable=NO |
| content_calendar.repeat_type | integer, nullable=NO |
| content_calendar.repeat_interval | integer, nullable=NO |
| content_calendar.repeat_until | timestamp with time zone, nullable=YES |
| content_calendar.next_scheduled_date | timestamp with time zone, nullable=YES |
| content_calendar.integration_ids | text, nullable=YES |
| content_calendar.profile_id | uuid, nullable=NO |
| content_calendar.is_active | boolean, nullable=NO |
| content_calendar.is_deleted | boolean, nullable=NO |
| content_calendar.created_at | timestamp with time zone, nullable=NO |
| content_calendar.updated_at | timestamp with time zone, nullable=NO |
| content_calendar.attempt_count | integer, nullable=NO |
| content_calendar.executed_at | timestamp with time zone, nullable=YES |
| content_calendar.integration_id | uuid, nullable=YES |
| content_calendar.last_error | text, nullable=YES |
| content_calendar.scheduled_at | timestamp with time zone, nullable=YES |
| content_calendar.status | integer, nullable=NO |
| content_calendar.workspace_id | uuid, nullable=NO |
| content_participations.id | uuid, nullable=NO |
| content_participations.workspace_id | uuid, nullable=NO |
| content_participations.content_id | uuid, nullable=NO |
| content_participations.user_id | uuid, nullable=NO |
| content_participations.recorded_by | uuid, nullable=NO |
| content_participations.created_at | timestamp with time zone, nullable=NO |
| contents.primary_creator_id | uuid, nullable=YES |
| contents.team_id | uuid, nullable=YES |
| posts.id | uuid, nullable=NO |
| posts.content_id | uuid, nullable=NO |
| posts.integration_id | uuid, nullable=NO |
| posts.external_post_id | character varying, nullable=YES |
| posts.published_at | timestamp with time zone, nullable=NO |
| posts.status | integer, nullable=NO |
| posts.is_deleted | boolean, nullable=NO |
| posts.created_at | timestamp with time zone, nullable=NO |
| team_brands.id | uuid, nullable=NO |
| team_brands.team_id | uuid, nullable=NO |
| team_brands.brand_id | uuid, nullable=NO |
| team_brands.assigned_at | timestamp with time zone, nullable=NO |
| team_brands.is_active | boolean, nullable=NO |
| team_brands.channel_access_mode | integer, nullable=NO |
| team_channel_access.id | uuid, nullable=NO |
| team_channel_access.team_brand_id | uuid, nullable=NO |
| team_channel_access.integration_id | uuid, nullable=NO |
| team_members.id | uuid, nullable=NO |
| team_members.team_id | uuid, nullable=NO |
| team_members.user_id | uuid, nullable=NO |
| team_members.role | character varying, nullable=NO |
| team_members.permissions | jsonb, nullable=NO |
| team_members.joined_at | timestamp with time zone, nullable=NO |
| team_members.is_active | boolean, nullable=NO |
| teams.id | uuid, nullable=NO |
| teams.profile_id | uuid, nullable=YES |
| teams.name | character varying, nullable=NO |
| teams.description | character varying, nullable=YES |
| teams.is_deleted | boolean, nullable=NO |
| teams.status | integer, nullable=NO |
| teams.created_at | timestamp with time zone, nullable=NO |
| teams.updated_at | timestamp with time zone, nullable=YES |
| teams.workspace_id | uuid, nullable=NO |

## Table counts

| Metric / field | Value / definition |
|---|---|
| teams | 66 |
| team_brands | 46 |
| team_members | 72 |
| contents | 544 |
| posts | 179 |
| assets | 0 |
| schedules | 139 |
| integrations | 147 |

## Migration risks

| Metric / field | Value / definition |
|---|---|
| teams_without_workspace_via_legacy_profile | 66 |
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
| team_profile_missing | 66 |
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

