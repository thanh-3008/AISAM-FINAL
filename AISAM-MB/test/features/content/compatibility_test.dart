import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/content/data/models/content_model.dart';

Map<String,dynamic> fixture() => {
  'id':'id','profileId':'profile','brandId':'brand','adType':0,
  'isAiGenerated':false,'status':0,'createdAt':'2026-09-10T00:00:00Z',
  'textContent':'legacy',
};
void main() {
  test('canonical plain text takes precedence without parsing HTML',() {
    final model=ContentResponseModel.fromJson({...fixture(),'plainText':'<script>alert(1)</script>','richTextVersion':99});
    expect(model.textContent,'<script>alert(1)</script>');
    expect(model.updatedAt,model.createdAt);
  });
  test('legacy single image and ordered JSON array are supported',() {
    final single=ContentResponseModel.fromJson({...fixture(),'imageUrl':'https://cdn.test/a'});
    final multiple=ContentResponseModel.fromJson({...fixture(),'imageUrl':'["https://cdn.test/b","javascript:bad","https://cdn.test/a"]'});
    expect(single.legacyImageUrls,['https://cdn.test/a']);
    expect(multiple.legacyImageUrls,['https://cdn.test/b','https://cdn.test/a']);
    expect(single.textContent,'legacy');
  });
}
