import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/core/network/rbac_context.dart';
import 'package:aisam_mb/features/content/data/models/content_request.dart';
import 'package:aisam_mb/features/content/data/models/ai_generation_request.dart';
import 'package:aisam_mb/features/content/data/models/enums.dart';

Map<String, dynamic> contextJson(Object? role) => {
  'contractVersion': 2,
  'revision': 'rev-1',
  'workspaceRole': role,
  'actions': <String>[],
  'scopes': <Map<String, dynamic>>[
    {
      'teamId': 'team-a',
      'brandId': 'brand-a',
      'role': 'ContentCreator',
      'channelIds': <String>[],
    },
    {
      'teamId': 'team-b',
      'brandId': 'brand-b',
      'role': 'Viewer',
      'channelIds': <String>[],
    },
  ],
};

void main() {
  test('legacy numeric roles never become v2 authority', () {
    for (final role in [1, 2, 3, 4, 'Manager', 'ContentCreator', null]) {
      expect(
        () => RbacContext.fromJson(contextJson(role)),
        throwsFormatException,
      );
    }
  });
  test('revision-only legacy context grants no v2 capability', () {
    final access = RbacContext.fromJson({'revision': 'legacy'});
    expect(access.isV2, false);
    expect(
      access.canCreate(TeamScope('a', 'b', TeamRoleV2.manager, [])),
      false,
    );
  });
  test('creator rights in one Team do not grant write access in another', () {
    final access = RbacContext.fromJson(contextJson('Member'));
    expect(access.canCreate(access.scopes.first), true);
    expect(access.canCreate(access.scopes.last), false);
    expect(
      access.canCreate(TeamScope('foreign', 'brand', TeamRoleV2.manager, [])),
      false,
    );
  });
  test(
    'workspace administrators still need a real assigned Team to create',
    () {
      for (final role in ['Owner', 'WorkspaceManager']) {
        final json = contextJson(role);
        (json['scopes'] as List).add({
          'teamId': '00000000-0000-0000-0000-000000000000',
          'brandId': 'unassigned',
          'role': null,
          'channelIds': <String>[],
        });
        final access = RbacContext.fromJson(json);
        expect(access.canCreate(access.scopes.first), true);
        expect(access.canCreate(access.scopes.last), false);
      }
    },
  );
  test('unknown Team role and contract version fail closed', () {
    final json = contextJson('Member');
    (json['scopes'] as List).first['role'] = 'Owner';
    expect(() => RbacContext.fromJson(json), throwsFormatException);
    expect(
      () =>
          RbacContext.fromJson({...contextJson('Owner'), 'contractVersion': 3}),
      throwsFormatException,
    );
  });
  test(
    'manual and AI creation serialize selected Team without modifying edit contract',
    () {
      const manual = CreateContentRequest(
        brandId: 'brand-a',
        teamId: 'team-a',
        adType: AdTypeEnum.textOnly,
        textContent: 'draft',
      );
      const ai = CreateDraftRequest(
        brandId: 'brand-a',
        teamId: 'team-a',
        adType: AdTypeEnum.textOnly,
        prompt: 'draft',
      );
      expect(manual.toJson()['teamId'], 'team-a');
      expect(CreateContentRequest.fromJson(manual.toJson()).teamId, 'team-a');
      expect(ai.toJson()['teamId'], 'team-a');
      expect(CreateDraftRequest.fromJson(ai.toJson()).teamId, 'team-a');
      expect(
        const UpdateContentRequest(
          textContent: 'edit',
        ).toJson().containsKey('teamId'),
        false,
      );
    },
  );
}
