param([string]$PgBin = 'C:\Program Files\PostgreSQL\18\bin')
$ErrorActionPreference = 'Stop'
if (-not $env:PGPASSWORD) { throw 'Set PGPASSWORD in this terminal before running.' }
$env:PGHOST = '127.0.0.1'
$env:PGPORT = '5432'
$env:PGUSER = 'postgres'
$env:PGCONNECT_TIMEOUT = '15'
$taskRoot = Split-Path $PSScriptRoot -Parent
$stamp = Get-Date -Format 'yyyyMMddHHmmss'
$output = Join-Path $taskRoot ".artifacts/r12/$stamp"
New-Item -ItemType Directory -Path $output -Force | Out-Null
$dump = Join-Path $output 'aisam_local.dump'
$restoreDb = "aisam_r12_restore_$stamp"
function Pg([string]$name, [string[]]$arguments) {
    $result = & (Join-Path $PgBin "$name.exe") @arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        $result | Out-File (Join-Path $output "$name-error.log") -Encoding utf8
        throw "$name failed; local diagnostic retained under $output"
    }
    return $result
}
# Dump source read-only. Never restore over the application's database.
Pg 'pg_dump' @('--no-password','--format=custom','--no-owner','--no-acl','--dbname=aisam_local',"--file=$dump") | Out-Null
Pg 'createdb' @('--no-password',$restoreDb) | Out-Null
Pg 'pg_restore' @('--no-password','--exit-on-error','--no-owner','--no-acl',"--dbname=$restoreDb",$dump) | Out-Null
$sql = @'
SELECT 'contents',count(*) FROM contents
UNION ALL SELECT 'workspace_members',count(*) FROM workspace_members
UNION ALL SELECT 'teams',count(*) FROM teams
UNION ALL SELECT 'team_members',count(*) FROM team_members
UNION ALL SELECT 'team_brands',count(*) FROM team_brands
UNION ALL SELECT 'posts',count(*) FROM posts
ORDER BY 1;
'@
$sqlFile = Join-Path $output 'counts.sql'
$sql | Set-Content $sqlFile -Encoding utf8
$sourceCounts = @(Pg 'psql' @('--no-password','-X','-At','-v','ON_ERROR_STOP=1','-d','aisam_local','-f',$sqlFile))
$restoredCounts = @(Pg 'psql' @('--no-password','-X','-At','-v','ON_ERROR_STOP=1','-d',$restoreDb,'-f',$sqlFile))
if (Compare-Object $sourceCounts $restoredCounts) { throw 'Source/restore counts differ; inspect concurrent source writes before rollout.' }
$report = [ordered]@{
    checkedAtUtc = [DateTime]::UtcNow.ToString('o')
    source = '127.0.0.1:5432/aisam_local'
    restoredDatabase = $restoreDb
    dumpSha256 = (Get-FileHash $dump -Algorithm SHA256).Hash
    verifiedCounts = $restoredCounts
    result = 'PASS backup restored; selected table counts match'
    limitation = 'No migration or application writes. Not a checksum of every row; source may change after dump.'
}
$report | ConvertTo-Json | Set-Content (Join-Path $output 'backup-report.json') -Encoding utf8
Write-Output "PASS backup/restore: $restoreDb; report: $output/backup-report.json"
