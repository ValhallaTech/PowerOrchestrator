# PowerOrchestrator Sample Scripts

This directory contains example PowerShell scripts for testing and development.

## Scripts

- `hello-world.ps1` - Basic greeting script
- `system-info.ps1` - System information gathering
- `file-operations.ps1` - File and directory operations
- `network-check.ps1` - Network connectivity testing
- `service-management.ps1` - Windows service management

## Usage

These scripts are automatically discovered by PowerOrchestrator when:
1. Connected to a GitHub repository
2. Scripts are placed in a designated folder
3. Scripts follow the naming convention and include proper metadata

## Metadata Format

Each script should include metadata in comments:

```powershell
<#
.SYNOPSIS
    Brief description of the script
.DESCRIPTION
    Detailed description of what the script does
.PARAMETER Name
    Description of parameters
.EXAMPLE
    Example usage
.NOTES
    Additional notes, version, author, etc.
#>
```