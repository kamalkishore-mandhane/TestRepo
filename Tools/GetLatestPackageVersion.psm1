<#
.SYNOPSIS
   Get nuget package latest version from GitHub.

.DESCRIPTION
   This script gets current latest version from GitHub Packages using GitHub Packages API.

.PARAMETER GitHubToken
   GitHub personal access token for authentication.

.PARAMETER PackageName
   Package name to get current latest version from GitHub Packages.

.EXAMPLE
   GetLatestPackageVersion -GitHubToken "YOUR_GITHUB_TOKEN" -PackageName "WzApplets"
#>

function GetLatestPackageVersion
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string] $GitHubToken,

        [Parameter(Mandatory = $true)]
        [string] $PackageName
    )

    try
    {
        $url = "https://api.github.com/orgs/Alludo-WinZip/packages/nuget/$PackageName/versions"

        $headers = @{ 'Authorization' = "token $GitHubToken" }

        $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get

        $latestVersion = $response | Sort-Object { [Version]($_.name) } -Descending | Select-Object -First 1

        if ($latestVersion)
        {
            return $latestVersion.name
        }
        else
        {
            return $null
        }
    }
    catch
    {
        Write-Error "An error occurred: $_"
        return $null
    }
}