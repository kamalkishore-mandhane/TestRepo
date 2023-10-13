@echo OFF

setlocal 

if exist "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64" goto PF86

@echo WARNING Microsoft SDK 10.0A not found !
goto :EOF

:PF86
set PATH=%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64
goto :MUI_BUILD

:MUI_BUILD

Bat\ResExtract%2 %1\ImgUtil%2.exe ImgUtil%2.g.resources
call bat\MUI_Build.bat %1 de-DE ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 en-US ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 es-ES ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 es-MX ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 fr-FR ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 it-IT ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 ja-JP ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 nl-NL ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 pt-BR ImgUtil%2 ImgUtil %2
call bat\MUI_Build.bat %1 zh-CN ImgUtil%2 ImgUtil %2
                                                  
endlocal
