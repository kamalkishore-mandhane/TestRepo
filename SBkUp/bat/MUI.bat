@echo OFF

setlocal 

if exist "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64" goto PF86

@echo WARNING Microsoft SDK 10.0A not found !
goto :EOF

:PF86
set PATH=%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64
goto :MUI_BUILD

:MUI_BUILD

set BuildFolder=%1
set BuildAssembly=%2

if [%BuildFolder%]==[] goto USAGE
if [%BuildAssembly%]==[] goto USAGE

Bat\ResExtract %BuildFolder%\%BuildAssembly%.dll %BuildAssembly%.g.resources

call bat\MUI_Build.bat %BuildFolder% de-DE %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% en-US %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% es-ES %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% es-MX %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% fr-FR %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% it-IT %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% ja-JP %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% nl-NL %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% pt-BR %BuildAssembly%
call bat\MUI_Build.bat %BuildFolder% zh-CN %BuildAssembly%

goto :DONE

:USAGE
@echo Usage:    %0 [BuildFolder] [BuildAssembly]
@echo Example:  %0 w64prod CloudStoragePicker

:DONE
set BuildFolder=
Set BuildAssembly=

endlocal
