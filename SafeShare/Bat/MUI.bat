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
set BuildBit=%3

Bat\ResExtract%BuildBit% %BuildFolder%\%BuildAssembly%%BuildBit%.exe %BuildAssembly%%BuildBit%.g.resources

call bat\MUI_Build.bat %BuildFolder% de-DE %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% en-US %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% es-ES %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% es-MX %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% fr-FR %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% it-IT %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% ja-JP %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% nl-NL %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% pt-BR %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% zh-CN %BuildAssembly% %BuildBit%

goto :DONE

:DONE
set BuildFolder=
Set BuildAssembly=
set BuildBit=

endlocal
