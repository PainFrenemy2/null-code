# Null Monitor — установка одной командой:
#   irm https://raw.githubusercontent.com/PainFrenemy2/null-code/null-monitor/install.ps1 | iex
$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Write-Host 'Null Monitor: ищу последнюю версию...' -ForegroundColor Magenta
# Assign first: in Windows PowerShell 5.1 the JSON array must be stored before piping it.
$releases = Invoke-RestMethod 'https://api.github.com/repos/PainFrenemy2/null-code/releases?per_page=50' -Headers @{ 'User-Agent' = 'NullMonitor-Install' }
$release = $releases | Where-Object { -not $_.draft -and -not $_.prerelease -and $_.tag_name -like 'null-monitor-v*' } | Select-Object -First 1
if (-not $release) { throw 'Релиз Null Monitor не найден.' }

$asset = $release.assets | Where-Object { $_.name -eq 'NullMonitor-Setup.exe' } | Select-Object -First 1
if (-not $asset) { throw 'В релизе нет установщика.' }

$file = Join-Path $env:TEMP 'NullMonitor-Setup.exe'
Write-Host "Скачиваю $($release.name)..." -ForegroundColor Cyan
Invoke-WebRequest $asset.browser_download_url -OutFile $file -UseBasicParsing

Write-Host 'Запускаю установщик...' -ForegroundColor Cyan
Start-Process $file -Wait
Remove-Item $file -ErrorAction SilentlyContinue
Write-Host 'Готово! Смотри на панель задач возле часов.' -ForegroundColor Green
