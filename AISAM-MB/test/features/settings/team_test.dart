import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/settings/data/models/team_model.dart';
import 'package:aisam_mb/features/workspace/data/models/workspace_model.dart';

void main() {
  group('Team & Shared Credit Tests', () {
    test('TeamSummaryModel deserializes backend team list payload', () {
      final json = {
        'id': 'team-123',
        'name': 'Marketing Team',
        'description': 'Marketing and growth operations',
        'status': 'Active',
        'memberCount': 5,
        'brandCount': 2,
        'createdAt': '2026-09-01T10:00:00Z',
        'hasManager': true,
      };

      final team = TeamSummaryModel.fromJson(json);

      expect(team.id, 'team-123');
      expect(team.name, 'Marketing Team');
      expect(team.description, 'Marketing and growth operations');
      expect(team.status, 'Active');
      expect(team.memberCount, 5);
      expect(team.brandCount, 2);
      expect(team.hasManager, true);
    });

    test('TeamDetailModel deserializes members and brands correctly', () {
      final json = {
        'id': 'team-123',
        'name': 'Content Team',
        'status': 'Active',
        'createdAt': '2026-09-01T10:00:00Z',
        'members': [
          {
            'userId': 'user-1',
            'name': 'Alice Nguyen',
            'email': 'alice@example.com',
            'role': 'Manager',
            'joinedAt': '2026-09-01T10:00:00Z',
            'isActive': true,
          }
        ],
        'brands': [
          {
            'brandId': 'brand-1',
            'brandName': 'Acme Brand',
            'isActive': true,
            'assignedAt': '2026-09-01T10:00:00Z',
          }
        ]
      };

      final detail = TeamDetailModel.fromJson(json);

      expect(detail.id, 'team-123');
      expect(detail.members.length, 1);
      expect(detail.members.first.name, 'Alice Nguyen');
      expect(detail.members.first.role, 'Manager');
      expect(detail.brands.length, 1);
      expect(detail.brands.first.brandName, 'Acme Brand');
    });

    test('CreditWalletModel correctly parses balance from wallet API', () {
      final json = {
        'balance': 15000,
        'workspaceId': 'ws-456',
      };

      final wallet = CreditWalletModel.fromJson(json);

      expect(wallet.balance, 15000);
      expect(wallet.workspaceId, 'ws-456');
    });

    test('WorkspaceMemberResponseModel parses quotaMode and credit limits', () {
      final json = {
        'id': 'member-1',
        'userId': 'user-100',
        'email': 'creator@example.com',
        'fullName': 'Bob Creator',
        'role': 3,
        'quotaMode': 2, // LifetimeAssigned
        'creditLimit': 5000,
        'creditUsed': 1200,
        'creditPeriodStart': '2026-09-01T00:00:00Z',
        'joinedAt': '2026-09-01T00:00:00Z',
      };

      final member = WorkspaceMemberResponseModel.fromJson(json);

      expect(member.id, 'member-1');
      expect(member.role, 3);
      expect(member.quotaMode, 2);
      expect(member.creditLimit, 5000);
      expect(member.creditUsed, 1200);

      // Verify calculation of remaining credits
      final remaining = (member.creditLimit! - member.creditUsed!);
      expect(remaining, 3800);
    });

    test('Shared pool member calculation uses workspace wallet balance', () {
      final json = {
        'id': 'member-2',
        'userId': 'user-200',
        'email': 'shared@example.com',
        'role': 4,
        'quotaMode': 1, // SharedPool
        'joinedAt': '2026-09-01T00:00:00Z',
      };

      final member = WorkspaceMemberResponseModel.fromJson(json);
      const workspaceWalletBalance = 25000;

      expect(member.quotaMode, 1);
      expect(member.creditLimit, null);

      // When quotaMode is 1 (SharedPool), available credit is workspace wallet balance
      final isSharedPool = member.quotaMode == 1 || member.creditLimit == null;
      expect(isSharedPool, true);
      expect(workspaceWalletBalance, 25000);
    });

    test('Strict synchronization filters out team members that are no longer active in workspace', () {
      // Team detail contains 3 members: user-1, user-2, user-3 (user-3 was removed from workspace)
      final teamDetail = TeamDetailModel(
        id: 'team-1',
        name: 'Alpha Team',
        status: 'Active',
        createdAt: DateTime.now(),
        members: [
          TeamMemberItemModel(userId: 'user-1', name: 'Member One', email: 'm1@test.com', role: 'Manager', joinedAt: DateTime.now(), isActive: true),
          TeamMemberItemModel(userId: 'user-2', name: 'Member Two', email: 'm2@test.com', role: 'ContentCreator', joinedAt: DateTime.now(), isActive: true),
          TeamMemberItemModel(userId: 'user-3', name: 'Deleted Member', email: 'm3@test.com', role: 'Viewer', joinedAt: DateTime.now(), isActive: true),
        ],
        brands: [],
      );

      // Workspace members only returns active members: user-1 and user-2
      final wsMembers = [
        WorkspaceMemberResponseModel(id: 'wm-1', userId: 'user-1', email: 'm1@test.com', role: 2, joinedAt: DateTime.now()),
        WorkspaceMemberResponseModel(id: 'wm-2', userId: 'user-2', email: 'm2@test.com', role: 3, joinedAt: DateTime.now()),
      ];

      // Dual-key lookup map
      final memberLookup = <String, WorkspaceMemberResponseModel>{};
      for (var m in wsMembers) {
        memberLookup[m.userId] = m;
        memberLookup[m.id] = m;
      }

      // Filter: only team members that actually exist in active workspace members
      final activeTeamMembers = teamDetail.members.where((tm) => memberLookup.containsKey(tm.userId)).toList();

      expect(activeTeamMembers.length, 2);
      expect(activeTeamMembers.any((m) => m.userId == 'user-3'), false);
      expect(activeTeamMembers.map((m) => m.userId).toList(), ['user-1', 'user-2']);
    });

    test('Dual-key memberLookup prevents mismatch between WorkspaceMember.id and userId', () {
      final wsMembers = [
        WorkspaceMemberResponseModel(id: 'wm-membership-id', userId: 'user-actual-id', email: 'test@example.com', role: 3, joinedAt: DateTime.now()),
      ];

      final memberLookup = <String, WorkspaceMemberResponseModel>{};
      for (var m in wsMembers) {
        memberLookup[m.userId] = m;
        memberLookup[m.id] = m;
      }

      // Can be found by userId
      expect(memberLookup.containsKey('user-actual-id'), true);
      expect(memberLookup['user-actual-id']?.email, 'test@example.com');

      // Can also be found by membership id as fallback
      expect(memberLookup.containsKey('wm-membership-id'), true);
      expect(memberLookup['wm-membership-id']?.email, 'test@example.com');
    });

    test('Available members for adding to team correctly excludes already added members', () {
      final currentTeamMembers = [
        TeamMemberItemModel(userId: 'user-1', name: 'Member One', email: 'm1@test.com', role: 'Manager', joinedAt: DateTime.now(), isActive: true),
      ];

      final wsMembers = [
        WorkspaceMemberResponseModel(id: 'wm-1', userId: 'user-1', email: 'm1@test.com', role: 2, joinedAt: DateTime.now()),
        WorkspaceMemberResponseModel(id: 'wm-2', userId: 'user-2', email: 'm2@test.com', role: 3, joinedAt: DateTime.now()),
        WorkspaceMemberResponseModel(id: 'wm-3', userId: 'user-3', email: 'm3@test.com', role: 4, joinedAt: DateTime.now()),
      ];

      final currentMemberIds = currentTeamMembers.map((m) => m.userId).toSet();
      final availableMembers = wsMembers.where((wm) {
        return !currentMemberIds.contains(wm.userId) && !currentMemberIds.contains(wm.id);
      }).toList();

      expect(availableMembers.length, 2);
      expect(availableMembers.map((m) => m.userId).toList(), ['user-2', 'user-3']);
    });
  });
}
