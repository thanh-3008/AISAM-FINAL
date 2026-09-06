# AISAM Content Ownership Boundary

## Invariant

`Content.TeamId` is the persisted owner of a content resource. Collaboration,
assignment, temporary access, member transfer, and current brand membership never
change it. `WorkspaceId + TeamId` is protected by a composite foreign key.

New authenticated or attributed background writes must resolve one active owning
team that has an active `TeamBrand` link for the content brand. A missing or
ambiguous team fails closed.

## Creation-path audit

| Creation path | Ownership source | Boundary behavior |
| --- | --- | --- |
| `ContentService.CreateInWorkspaceAsync` | Server-resolved `AccessScope.ActiveTeamId`, or the single accessible team for the brand | Persistence rejects missing, ambiguous, foreign-workspace, deleted, or brand-ineligible team |
| Legacy `ContentService.CreateAsync` | Brand workspace plus the same server-resolved scope | No longer creates a workspace-less row; production persistence applies the same boundary |
| `ContentService.CloneAsync` and `CloneInWorkspaceAsync` | Existing content `TeamId` | Clone remains in the source ownership boundary; source ownership is unchanged |
| `AIService.GenerateDraftAsync` | Request access scope | Repository save applies the boundary |
| AI chat image/video/original-image draft paths | Request access scope | Repository save applies the boundary, including calls using `CancellationToken.None` |
| `AutomationGenerationService` | Immutable `ExecutionOperation.TeamId` background attribution | Explicitly copies the attributed team; save revalidates active team/brand relation |
| Development `AdminToolsController` seed | Single active `TeamBrand` candidate | Ambiguous or unowned brands are skipped; no unresolved demo content is created |
| `PostInsightsSyncService.ImportFacebookPagePostsAsync` | None | Method is currently unreachable. If re-enabled, it must run in an authenticated/attributed scope; repository persistence then fails closed when ownership is ambiguous |

Direct authenticated `DbContext` creation is also covered by
`PrepareContentOwnershipAsync`; it is not dependent on callers remembering to set
the property.

## Legacy reconciliation strategy

The rollout is intentionally expand–reconcile–contract:

1. Add nullable `contents.team_id` and the composite FK to
   `teams(id, workspace_id)` using `ON DELETE RESTRICT`.
2. In the migration, assign legacy rows only when the content's brand has exactly
   one active, non-deleted candidate team in the same workspace.
3. Keep shared-brand, missing-link, and otherwise ambiguous rows as `NULL`.
4. Treat unresolved rows as owner-only at team-scoped query boundaries. Their
   `PrimaryCreatorId` still permits historical view, but never edit/publish solely
   from historical attribution.
5. Reconcile remaining rows explicitly from trusted business records. The database
   trigger permits `NULL -> team_id` once, increments `PermissionRevision`, and
   rejects changing or clearing any established ownership.
6. After the unresolved count reaches zero and remains monitored, a later contract
   migration may make `team_id` non-null. This migration deliberately does not do so.

Operational inventory query:

```sql
SELECT workspace_id, brand_id, COUNT(*) AS unresolved_count
FROM contents
WHERE team_id IS NULL
GROUP BY workspace_id, brand_id
ORDER BY workspace_id, brand_id;
```

Reconciliation must use an explicit reviewed mapping of `content_id -> team_id`.
Current creator membership must not be used as a fallback because a prior team
transfer would silently transfer content ownership.

## Authorization boundary

- Owner: all workspace content, including unresolved legacy rows.
- Manager/Viewer: content must have an owning team in `AccessScope.TeamIds` and an
  allowed brand; channel-scoped children also require an allowed channel.
- Content Creator historical view: own/participated content remains visible in the
  same workspace after transfer.
- Content Creator mutation: current owning-team membership plus current brand/channel
  authorization is required.
- Assigned/temporary access remains an independent delegated-access mechanism and
  does not mutate `Content.TeamId`.

The same team predicate is applied to `Post` and `PerformanceReport` filters so
JOIN, SUM, COUNT, and other aggregates cannot bypass the content boundary.
