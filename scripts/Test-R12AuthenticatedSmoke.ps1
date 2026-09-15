$ErrorActionPreference = 'Stop'
if (-not $env:PGPASSWORD) { throw 'Set PGPASSWORD locally.' }
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root '.artifacts/r12'
$database = 'aisam_r12_restore_20260915184311'
$query = 'SELECT wm.user_id,wm.workspace_id,wm.workspace_role_v2 FROM workspace_members wm JOIN users u ON u.id=wm.user_id JOIN workspaces w ON w.id=wm.workspace_id WHERE wm.is_active AND u.is_active AND w.status=1 AND wm.workspace_role_v2 IN (1,3) ORDER BY wm.workspace_role_v2;'
$actors = @(& 'C:/Program Files/PostgreSQL/18/bin/psql.exe' -h 127.0.0.1 -U postgres -d $database -At -v ON_ERROR_STOP=1 -c $query)
if ($LASTEXITCODE -ne 0 -or $actors.Count -eq 0) { throw 'No eligible actors in fixed test clone.' }
$env:DOTNET_ROOT = 'C:\Users\thanh\.dotnet'
$env:ASPNETCORE_ENVIRONMENT = 'Testing'
$env:BackgroundJobs__Enabled = 'false'
$env:Rbac__UseV2 = 'true'
$env:CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=$database;Username=postgres;Password=$env:PGPASSWORD"
$env:JWT_SECRET_KEY = [Guid]::NewGuid().ToString('N') + [Guid]::NewGuid().ToString('N')
$env:JWT_ISSUER = 'r12-isolated'
$env:JWT_AUDIENCE = 'r12-isolated'
function Base64Url([byte[]]$bytes) { [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+','-').Replace('/','_') }
function Token([string]$id) {
    $header = Base64Url ([Text.Encoding]::UTF8.GetBytes('{"alg":"HS256","typ":"JWT"}'))
    $payload = @{sub=$id;iss='r12-isolated';aud='r12-isolated';role='User';exp=[DateTimeOffset]::UtcNow.AddMinutes(5).ToUnixTimeSeconds()} | ConvertTo-Json -Compress
    $body = $header + '.' + (Base64Url ([Text.Encoding]::UTF8.GetBytes($payload)))
    $hmac = [Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($env:JWT_SECRET_KEY))
    try { $body + '.' + (Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($body)))) } finally { $hmac.Dispose() }
}
$process = Start-Process -FilePath "$env:DOTNET_ROOT/dotnet.exe" -ArgumentList @('bin/Debug/net8.0/AISAM.API.dll','--urls','http://127.0.0.1:5128') -WorkingDirectory (Join-Path $root 'AISAM-BE/AISAM.API') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $out 'authenticated-api.log') -RedirectStandardError (Join-Path $out 'authenticated-api-error.log')
$results = @()
try {
    $ready = $false
    for ($i=0;$i -lt 30;$i++) {
        try { Invoke-WebRequest http://127.0.0.1:5128/api/health -UseBasicParsing -TimeoutSec 2 | Out-Null; $ready=$true; break } catch { Start-Sleep -Milliseconds 300 }
    }
    if (-not $ready) { throw 'Smoke API did not start.' }
    foreach ($actor in $actors) {
        $parts=$actor.Split('|')
        $headers=@{Authorization='Bearer '+(Token $parts[0]);'X-Workspace-Id'=$parts[1];'X-RBAC-Contract-Version'='2'}
        $response=Invoke-RestMethod http://127.0.0.1:5128/api/permissions/context -Headers $headers
        $expected=if($parts[2] -eq '1') {'Owner'} else {'Member'}
        if ($response.data.contractVersion -ne 2 -or $response.data.workspaceRole -ne $expected) { throw 'Permission context contract mismatch.' }
        $results += @{case='authenticated context';role=$expected;result='PASS'}
        $headers['X-Workspace-Id']=[Guid]::NewGuid().ToString()
        $status=0
        try { Invoke-WebRequest http://127.0.0.1:5128/api/permissions/context -Headers $headers -UseBasicParsing | Out-Null }
        catch { if($_.Exception.Response) {$status=[int]$_.Exception.Response.StatusCode} else {throw} }
        if ($status -notin 403,404) { throw "Foreign workspace not denied: $status" }
        $results += @{case='foreign workspace denied';role=$expected;status=$status;result='PASS'}
    }
    $results | ConvertTo-Json | Set-Content (Join-Path $out 'authenticated-smoke.json') -Encoding utf8
    Write-Output "PASS $($results.Count) HTTP checks against fixed clone; no provider calls."
} finally { if (-not $process.HasExited) { Stop-Process -Id $process.Id } }
