// Strongly typed enums for AISAM Scoped RBAC permissions.
// Matches BE definitions in AISAM.Services.Access (IAccessControlService.cs & ResourcePermission.cs).

enum AccessResourceKind {
  workspace(0),
  brand(1),
  content(2),
  channel(3),
  post(4);

  final int value;
  const AccessResourceKind(this.value);
}

enum ResourcePermission {
  brandView(0),
  brandManage(1),
  contentView(2),
  contentCreate(3),
  contentEdit(4),
  contentDelete(5),
  contentViewAllCreators(6),
  approvalReview(7),
  postPublish(8),
  postView(9),
  socialView(10),
  socialManage(11),
  analyticsView(12),
  analyticsMember(13),
  teamManage(14),
  billingManage(15),
  approvalWithdraw(16);

  final int value;
  const ResourcePermission(this.value);
}
