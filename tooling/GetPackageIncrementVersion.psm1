<#
.SYNOPSIS
   Generate new version number.

.DESCRIPTION
   This script generates new version for GitHub package based on current latest version on GitHub package and version increment type.

.PARAMETER CurrentPackageVersion
   Current latest version available on GitHub package,

.PARAMETER VersionIncrementType
   Version increment type. This value decides to increment major/minor/build version number. Value should be "major", "minor", "build".

.EXAMPLE
   GetPackageIncrementVersion -CurrentPackageVersion "1.0.0" -PackageName "build" 
   Above script will return new version number i.e. 1.0.1
#>

function GetPackageIncrementVersion
{
    [CmdletBinding()]
    param (
        [string] $CurrentPackageVersion,

        [Parameter(Mandatory = $true)]
        [string] $VersionIncrementType
    )
    try
    {
        $incrementMajor = 0
        $incrementMinor = 0
        $incrementBuild = 0
        
        switch ($VersionIncrementType)
        {
            "major" { $incrementMajor = 1; Break }
            "minor" { $incrementMinor = 1; Break }
            default { $incrementBuild = 1; Break }
        }
        
        $packageVersion = [version]$CurrentPackageVersion
        if($packageVersion)
        {
            return [version](($packageVersion.Major + $incrementMajor), ($packageVersion.Minor + $incrementMinor), ($packageVersion.Build + $incrementBuild) -join ".")
        }
        else
        {
            return $null
        }
    }
    catch
    {
        return $null
    }
}