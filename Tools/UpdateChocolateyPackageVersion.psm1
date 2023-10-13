<#
.SYNOPSIS
   Update chocolatey package configuration to create package with new version number.

.DESCRIPTION
   This script gets current latest version from GitHub Packages using GitHub Packages API and update increment version in nuget specification file.

.PARAMETER GitHubToken
   GitHub personal access token for authentication.

.PARAMETER SpecFilePath
   Nuget specification file path to update package new version.

.EXAMPLE
   UpdateChocolateyPackageVersion -GitHubToken "YOUR_GITHUB_TOKEN" -SpecFilePath ".\Tools"
#>

function UpdateChocolateyPackageVersion
{
	[CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string] $GitHubToken,

        [Parameter(Mandatory = $true)]
        [string] $SpecFilePath
    )

    Import-Module (Resolve-Path("$SpecFilePath\GetLatestPackageVersion.psm1"))
    Import-Module (Resolve-Path("$SpecFilePath\GetPackageIncrementVersion.psm1"))

    # Get nuget specification file path.
    [string]$NuSpecFile = (Get-ChildItem -path $SpecFilePath -Filter *.nuspec | Select-Object -First 1).fullname

    # Load nuget specification file contents to update package version.
    [xml]$xml = Get-Content -path $NuSpecFile -Raw
    $ns = [System.Xml.XmlNamespaceManager]::new($xml.NameTable)
    $ns.AddNamespace('nuspec', 'http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd')

    # Get package name from nuget specification file.
    $packageName = $xml.SelectSingleNode('/nuspec:package/nuspec:metadata/nuspec:id', $ns).InnerText

    # Get current latest version from github package
    $CurrentLatestVersion = GetLatestPackageVersion -GitHubToken $GitHubToken -PackageName $PackageName 

    # Increment version number
    $IncrementVersion = GetPackageIncrementVersion -CurrentPackageVersion $CurrentLatestVersion -VersionIncrementType "Build"

    # Update nuget specification file version.
    if($IncrementVersion)
    {
        $xml.SelectSingleNode('/nuspec:package/nuspec:metadata/nuspec:version', $ns).InnerText = $IncrementVersion.ToString()
    }
    else
    {
        $xml.SelectSingleNode('/nuspec:package/nuspec:metadata/nuspec:version', $ns).InnerText = "1.0.0"
    }

    $xml.Save($NuSpecFile)
}