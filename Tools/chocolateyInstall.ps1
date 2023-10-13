$rootDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)" | Split-Path
$toolsDir = -join($rootDir,"\Tools") 
$buildBinariesDir = -join($rootDir,"\Build\") 
$defaultChocoPackPath =  -join($env:ChocolateyInstall,"\lib\")

$nugetSpecFile = (Get-ChildItem -path $toolsDir -Filter *.nuspec | Select-Object -First 1);

#----------start - nuget specification file upate----------
[string]$NuSpecFile = (Get-ChildItem -path $toolsDir -Filter *.nuspec | Select-Object -First 1).fullname
[xml]$xml = Get-Content -path $NuSpecFile -Raw
$ns = [System.Xml.XmlNamespaceManager]::new($xml.NameTable)
$ns.AddNamespace('nuspec', 'http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd')
[version]$NewVersion = (Get-ChildItem -path $buildBinariesDir -Filter *.exe | Select-Object -First 1).VersionInfo.FileVersion

#set default version as 1.0.0 if executable file version is not configured
if($NewVersion.Major -eq 0){ $NewVersion = [version]"1.0.0" }	

$xml.SelectSingleNode('/nuspec:package/nuspec:metadata/nuspec:version', $ns).InnerText = $NewVersion.ToString()
$xml.Save($NuSpecFile)
Write-Host 'Nuget specification version updated.'
#----------end - nuget specification file upate----------

#----------start - create chocolatey pack----------
choco pack $nugetSpecFile.FullName --outputdirectory $toolsDir
Write-Host 'Created chocolatey package'
#----------create chocolatey pack----------

#----------end - start - install nuget pack----------
choco install $nugetSpecFile.BaseName -y --version $NewVersion.ToString() --source $toolsDir
Write-Host 'Installed chocolatey package'
#----------install nuget pack----------

#----------start - move binaries to application folder----------
$packagePath =  -join($defaultChocoPackPath,$nugetSpecFile.BaseName, "\*")
$installationPath = $args[0]

if ($installationPath -and (Test-Path -Path $installationPath)) { 
	Remove-Item -Path (-join($installationPath, "\*")) -Recurse
	$exclude = @('*.nupkg','*.nuspec')
	Copy-Item -Path $packagePath -Destination $installationPath -Exclude $exclude -Recurse
	Write-Host 'Moved binaries to application folder.'
}
else
{
	Write-Host "Folder not found. $installationPath"
}
#----------end - move binaries to application folder----------

#----------start - uninstall choco package----------
choco uninstall $nugetSpecFile.BaseName
#----------end - uninstall choco package----------