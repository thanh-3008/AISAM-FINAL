import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/core/config/env_config.dart';

void main() {
  test('API build override takes precedence over environment asset', () {
    dotenv.testLoad(fileInput: 'API_BASE_URL=https://asset.example/api');
    const override = String.fromEnvironment('API_BASE_URL');
    expect(EnvConfig.apiBaseUrl,
        override.isEmpty ? 'https://asset.example/api' : override);
  });
}
