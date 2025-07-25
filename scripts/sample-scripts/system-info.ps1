<#
.SYNOPSIS
    Gather comprehensive system information
.DESCRIPTION
    Collects detailed system information including hardware, OS, and performance metrics.
    Returns structured data suitable for monitoring and reporting.
.EXAMPLE
    .\system-info.ps1
.NOTES
    Version: 1.0.0
    Author: PowerOrchestrator Team
    Purpose: System monitoring and diagnostics
#>

[CmdletBinding()]
param()

Write-Host "Gathering system information..." -ForegroundColor Cyan

try {
    # Basic system information
    $computerInfo = Get-ComputerInfo -Property WindowsProductName, TotalPhysicalMemory, CsProcessors, WindowsVersion, WindowsBuildLabEx
    
    # Performance counters
    $cpuUsage = Get-Counter '\Processor(_Total)\% Processor Time' | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
    $memoryAvailable = Get-Counter '\Memory\Available MBytes' | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
    
    # Disk information
    $diskInfo = Get-WmiObject -Class Win32_LogicalDisk | Where-Object { $_.DriveType -eq 3 } | ForEach-Object {
        [PSCustomObject]@{
            Drive = $_.DeviceID
            Size = [math]::Round($_.Size / 1GB, 2)
            FreeSpace = [math]::Round($_.FreeSpace / 1GB, 2)
            PercentFree = [math]::Round(($_.FreeSpace / $_.Size) * 100, 2)
        }
    }
    
    # Network adapters
    $networkAdapters = Get-NetAdapter | Where-Object { $_.Status -eq 'Up' } | Select-Object Name, InterfaceDescription, LinkSpeed
    
    # Create result object
    $systemInfo = [PSCustomObject]@{
        ComputerName = $env:COMPUTERNAME
        OperatingSystem = $computerInfo.WindowsProductName
        Version = $computerInfo.WindowsVersion
        Build = $computerInfo.WindowsBuildLabEx
        TotalMemoryGB = [math]::Round($computerInfo.TotalPhysicalMemory / 1GB, 2)
        AvailableMemoryMB = [math]::Round($memoryAvailable, 2)
        CPUUsagePercent = [math]::Round($cpuUsage, 2)
        ProcessorCount = $computerInfo.CsProcessors.Count
        Disks = $diskInfo
        NetworkAdapters = $networkAdapters
        Timestamp = Get-Date
        PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    }
    
    # Display summary
    Write-Host "`nSystem Information Summary:" -ForegroundColor Green
    Write-Host "Computer: $($systemInfo.ComputerName)"
    Write-Host "OS: $($systemInfo.OperatingSystem)"
    Write-Host "Memory: $($systemInfo.TotalMemoryGB) GB total, $($systemInfo.AvailableMemoryMB) MB available"
    Write-Host "CPU Usage: $($systemInfo.CPUUsagePercent)%"
    Write-Host "Processor Count: $($systemInfo.ProcessorCount)"
    
    return $systemInfo
}
catch {
    Write-Error "Failed to gather system information: $($_.Exception.Message)"
    return @{
        Status = "Error"
        Message = $_.Exception.Message
        Timestamp = Get-Date
    }
}