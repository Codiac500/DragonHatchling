param(
    [Parameter(Mandatory)][int]$ProcessId,
    [ValidateRange(1, 1440)][int]$Minutes = 15,
    [ValidateRange(1, 60)][int]$IntervalSeconds = 30,
    [string]$OutputPath = 'artifacts/validation/milestone-4/resources.csv'
)
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputPath)
if (Test-Path -LiteralPath $output) { throw 'Choose a new OutputPath; existing measurements will not be overwritten or mixed.' }
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($output)) | Out-Null
$target = Get-Process -Id $ProcessId
$startTime = $target.StartTime
$processors = [Environment]::ProcessorCount
$clock = [Diagnostics.Stopwatch]::StartNew()
$previousTime = 0.0
$previousCpu = $target.TotalProcessorTime.TotalSeconds
$samples = [Collections.Generic.List[object]]::new()
while ($true) {
    $target = Get-Process -Id $ProcessId
    if ($target.StartTime -ne $startTime) { throw 'Process ID was reused.' }
    $elapsed = $clock.Elapsed.TotalSeconds
    $cpu = $target.TotalProcessorTime.TotalSeconds
    $interval = if ($samples.Count -eq 0) { $null } else { $elapsed - $previousTime }
    if ($null -ne $interval -and $interval -gt 2 * $IntervalSeconds) {
        Write-Warning "Sampling gap of $([Math]::Round($interval, 1)) seconds; continuous observation was interrupted. Investigate before claiming the desktop endurance gate."
    }
    $intervalCpu = if ($samples.Count -eq 0) { $null } else {
        100 * ($cpu - $previousCpu) / ($elapsed - $previousTime) / $processors
    }
    $sample = [pscustomobject]@{
        utc = [DateTime]::UtcNow.ToString('o')
        elapsedSeconds = [Math]::Round($elapsed, 3)
        intervalSeconds = $interval
        cpuSeconds = $cpu
        machineCpuPercent = $intervalCpu
        workingSetMiB = $target.WorkingSet64 / 1MB
        privateMiB = $target.PrivateMemorySize64 / 1MB
        handles = $target.HandleCount
        threads = $target.Threads.Count
        responding = $target.Responding
        logicalProcessors = $processors
    }
    $samples.Add($sample)
    # Persist every sample so interrupted sessions retain their evidence.
    $sample | Export-Csv -LiteralPath $output -NoTypeInformation -Append
    Write-Output ($sample | ConvertTo-Json -Compress)
    if (!$sample.responding) { throw 'Application is not responding.' }
    if ($elapsed -ge $Minutes * 60) { break }
    $previousTime = $elapsed
    $previousCpu = $cpu
    Start-Sleep -Seconds ([Math]::Min($IntervalSeconds, [Math]::Ceiling($Minutes * 60 - $elapsed)))
}
Write-Output "Recorded $($samples.Count) samples to $output. Review CPU, memory, handles and threads over time; responding alone does not prove desktop input works."
