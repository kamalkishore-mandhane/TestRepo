# Applets  
Applets is visual studio solution with projects DupFF, ImgUtil, PdfUtil, StartupPaneLib and Common.

## NuGet Packages used
We need to install following Nuget packages for projects:
Right click on project name, select Manage 'Manage NuGet Packages' and search for following packages in search text box. 
Click on Install package in right pane. It will install the package and add reference of the installed package to the selected project.

### DupFF
  - MSBuildTasks

### ImgUtil
  - Aspose.Imaging
  - Aspose.PSD
  - MSBuildTasks

### PdfUtil
  - Aspose.Imaging
  - Aspose.PDF
  - MSBuildTasks

### Target framework
  .Net Framework 4.6

### Build steps:
Open the Applets solution in VS 2022 and build the solution through visual studio.

ALternatively solution can be build using command prompt or Powershell command with command [check the solution path before running the command]
```powershell
dotnet build C:\Projects\Applets\Applets.sln
```
If that fails, try from the root of the repo:
```powershell
MSBuild Applets.sln
```

## Building chocolatey package for Applets
Run the PowerShell script `chocolateyInstall.ps1` placed in `Tools` directory to generate applet chocolatey binaries at specific directory location.

```powershell
PS [ToolsDirPath]> .\chocolateyInstall.ps1 [installation path]
```

#### Example
```powershell
PS C:\projects\applets\tools> .\chocolateyInstall.ps1 'c:\Program Files\WinZip\Applets'
```

This command will move all the build binaries into specified folder i.e. `c:\Program Files\WinZip\Applets`

## Future Development
It appears that the the Common classes are only used in DupFF. It isn't clear whether they are used in the SafeShare or SBkUp applets. If they are
not, those classes could be moved into DupFF and the Common folder/project deleted and the use of AppletCommon.dll removed.
