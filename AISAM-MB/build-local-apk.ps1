param([string]$ApiBaseUrl)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$flutterExe = Join-Path $repoRoot '.artifacts/flutter-sdk/bin/flutter.bat'
$sdkPath = Join-Path $repoRoot '.artifacts/android-sdk'
$javaPath = Join-Path $repoRoot '.artifacts/java/jdk-17.0.20.1+1'
foreach ($required in @($flutterExe, $sdkPath, (Join-Path $javaPath 'bin/java.exe'))) {
    if (!(Test-Path -LiteralPath $required)) { throw "Missing local build tool: $required" }
}
if (!(Test-Path -LiteralPath (Join-Path $PSScriptRoot '.env'))) {
    throw 'Create AISAM-MB/.env from .env.example and set the reachable API URL first.'
}
$previousJava = $env:JAVA_HOME
$previousAndroid = $env:ANDROID_HOME
try {
    $env:JAVA_HOME = $javaPath
    $env:ANDROID_HOME = $sdkPath
    Push-Location $PSScriptRoot
    try {
        # Windows PowerShell wraps native stderr warnings as ErrorRecords.
        # Judge the build by its exit code, not by whether it printed a warning.
        $ErrorActionPreference = 'Continue'
        $buildArgs = @('build', 'apk', '--debug', '--target-platform', 'android-arm64')
        if ($ApiBaseUrl) {
            $uri = $null
            if (![Uri]::TryCreate($ApiBaseUrl, [UriKind]::Absolute, [ref]$uri) -or
                $uri.Scheme -notin @('http', 'https') -or $uri.UserInfo) {
                throw 'ApiBaseUrl must be an HTTP(S) URL without embedded credentials.'
            }
            $buildArgs += "--dart-define=API_BASE_URL=$ApiBaseUrl"
        }
        & $flutterExe @buildArgs
        $buildExit = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        if ($buildExit -ne 0) { throw "APK build failed ($buildExit)." }
    } finally { Pop-Location }
} finally {
    $env:JAVA_HOME = $previousJava
    $env:ANDROID_HOME = $previousAndroid
}
