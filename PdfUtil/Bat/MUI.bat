@echo OFF

setlocal 

if exist "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64" goto PF86

@echo WARNING Microsoft SDK 10.0A not found !
goto :EOF

:PF86
set PATH=%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64

set argC=0
for %%x in (%*) do Set /A argC+=1

if %argC%==3 (goto MUI_BUILD1)
if %argC%==2 (goto MUI_BUILD2)

:MUI_BUILD1

set BuildFolder=%1
set BuildAssembly=%2
set BuildBit=%3

Bat\ResExtract%BuildBit% %BuildFolder%\%BuildAssembly%%BuildBit%.exe %BuildAssembly%%BuildBit%.g.resources
call bat\MUI_Build.bat %BuildFolder% de-DE %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% en-US %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% es-ES %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% es-MX %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% fr-FR %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% it-IT %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% ja-JP %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% nl-NL %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% pt-BR %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
call bat\MUI_Build.bat %BuildFolder% zh-CN %BuildAssembly%%BuildBit% %BuildAssembly% %BuildBit%
goto DONE

:MUI_BUILD2

set BuildFolder=%1
set BuildAssembly=%2

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
goto DONE

:DONE
endlocal
