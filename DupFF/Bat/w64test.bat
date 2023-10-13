@echo OFF

setlocal
call %_LOCAL_SETENV_% ATL BOOST
MSBuild "DupFF.sln" /t:Rebuild /p:Configuration=Debug /p:Platform="x64"

xcopy bin\Debug\x64\*.exe w64test\ /s /h /d /y
xcopy bin\Debug\x64\*.dll w64test\ /s /h /d /y
xcopy bin\Debug\x64\*.xml w64test\ /s /h /d /y
xcopy bin\Debug\x64\*.pdb w64test\ /s /h /d /y
xcopy bin\Debug\x64\*.config w64test\ /s /h /d /y

call bat\MUI.bat w64test 64
@echo WzBuild Error Level is %ERRORLEVEL%
if errorlevel 1 goto DONE
if exist bat\private_out.bat call bat\private_out.bat TEST 64

:DONE
endlocal
