import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/core/network/rbac_context.dart';
import 'package:aisam_mb/shared/widgets/rbac_screen_gate.dart';
import 'package:aisam_mb/shared/widgets/team_scope_field.dart';

void main() {
  testWidgets(
    'legacy screens stay available and v2-only unsupported forms are not built',
    (tester) async {
      var builds = 0;
      Widget app(RbacContext access) => ProviderScope(
        overrides: [rbacContextProvider.overrideWith((ref) async => access)],
        child: MaterialApp(
          home: RbacScreenGate(
            unsupportedV2: 'Use website for Team administration.',
            child: Builder(
              builder: (_) {
                builds++;
                return const Text('legacy form');
              },
            ),
          ),
        ),
      );
      await tester.pumpWidget(app(RbacContext('legacy', null, [], [], false)));
      await tester.pumpAndSettle();
      expect(find.text('legacy form'), findsOneWidget);
      await tester.pumpWidget(const SizedBox());
      builds = 0;
      await tester.pumpWidget(
        app(RbacContext('v2', WorkspaceRoleV2.owner, [], [], true)),
      );
      await tester.pumpAndSettle();
      expect(builds, 0);
      expect(find.text('Use website for Team administration.'), findsOneWidget);
    },
  );

  testWidgets('context error offers retry without rendering protected form', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          rbacContextProvider.overrideWith(
            (ref) async => throw StateError('denied'),
          ),
        ],
        child: const MaterialApp(home: RbacScreenGate(child: Text('private'))),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.text('private'), findsNothing);
    expect(find.text('Thử lại'), findsOneWidget);
  });

  testWidgets(
    'Team chooser excludes Viewer Team and Teams belonging to another Brand',
    (tester) async {
      final scopes = [
        TeamScope('creator', 'brand', TeamRoleV2.contentCreator, []),
        TeamScope('viewer', 'brand', TeamRoleV2.viewer, []),
        TeamScope('other-brand', 'other', TeamRoleV2.manager, []),
      ];
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            rbacContextProvider.overrideWith(
              (ref) async =>
                  RbacContext('v2', WorkspaceRoleV2.member, scopes, [], true),
            ),
          ],
          child: MaterialApp(
            home: Scaffold(
              body: TeamScopeField(
                brandId: 'brand',
                value: null,
                onChanged: (_) {},
              ),
            ),
          ),
        ),
      );
      await tester.pumpAndSettle();
      final field = tester.widget<DropdownButtonFormField<String>>(
        find.byType(DropdownButtonFormField<String>),
      );
      expect(field.initialValue, null);
      await tester.tap(find.byType(DropdownButtonFormField<String>));
      await tester.pumpAndSettle();
      expect(find.text('creator'), findsWidgets);
      expect(find.text('viewer'), findsNothing);
      expect(find.text('other-brand'), findsNothing);
    },
  );
}
