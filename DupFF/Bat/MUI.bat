@echo OFF

setlocal 

if exist "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64" goto PF86

@echo WARNING Microsoft SDK 10.0A not found !
goto :EOF

:PF86
set PATH=%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64
goto :MUI_BUILD

:MUI_BUILD

Bat\ResExtract%2 %1\DupFF%2.exe DupFF%2.g.resources
call bat\MUI_Build.bat %1 de-DE DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 en-US DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 es-ES DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 es-MX DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 fr-FR DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 it-IT DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 ja-JP DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 nl-NL DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 pt-BR DupFF%2 DupFF %2
call bat\MUI_Build.bat %1 zh-CN DupFF%2 DupFF %2
                                                  
endlocal
