<#
.SYNOPSIS
    PowerOrchestrator Hello World demonstration script
.DESCRIPTION
    A simple script to demonstrate PowerOrchestrator execution capabilities.
    Displays a greeting message with timestamp and system information.
.PARAMETER Name
    The name to include in the greeting (optional)
.EXAMPLE
    .\hello-world.ps1
    .\hello-world.ps1 -Name "PowerShell Developer"
.NOTES
    Version: 1.0.0
    Author: PowerOrchestrator Team
    Purpose: Development and testing
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Name = "World"
)

# Display greeting
Write-Host "Hello, $Name!" -ForegroundColor Green
Write-Host "Welcome to PowerOrchestrator!" -ForegroundColor Cyan

# Show execution details
Write-Host "`nExecution Details:" -ForegroundColor Yellow
Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)"
Write-Host "Execution Policy: $(Get-ExecutionPolicy)"
Write-Host "Current User: $env:USERNAME"
Write-Host "Computer Name: $env:COMPUTERNAME"

# Return success status
return @{
    Status = "Success"
    Message = "Hello World script executed successfully"
    Timestamp = Get-Date
    User = $env:USERNAME
}