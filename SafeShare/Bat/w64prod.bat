@echo OFF

setlocal
call %_LOCAL_SETENV_% ATL BOOST
devenv "SafeShare.sln" /rebuild "Release|x64" 

xcopy bin\Release\x64\*.exe w64prod\ /s /h /d /y
xcopy bin\Release\x64\*.dll w64prod\ /s /h /d /y
xcopy bin\Release\x64\*.xml w64prod\ /s /h /d /y
xcopy bin\Release\x64\*.pdb w64prod\ /s /h /d /y
xcopy bin\Release\x64\*.config w64prod\ /s /h /d /y

call bat\MUI.bat w64prod SafeShare 64

@echo WzBuild Error Level is %ERRORLEVEL%
if errorlevel 1 goto DONE
if exist bat\private_out.bat call bat\private_out.bat PROD 64

:DONE
endlocal
