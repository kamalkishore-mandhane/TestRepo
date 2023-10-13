@echo OFF

setlocal
call %_LOCAL_SETENV_% ATL BOOST
MSBuild "DupFF.sln" /t:Rebuild /p:Configuration=Debug /p:Platform="x86"

xcopy bin\Debug\x86\*.exe w32test\ /s /h /d /y
xcopy bin\Debug\x86\*.dll w32test\ /s /h /d /y
xcopy bin\Debug\x86\*.xml w32test\ /s /h /d /y
xcopy bin\Debug\x86\*.pdb w32test\ /s /h /d /y
xcopy bin\Debug\x86\*.config w32test\ /s /h /d /y

call bat\MUI.bat w32test 32
@echo WzBuild Error Level is %ERRORLEVEL%
if errorlevel 1 goto DONE
if exist bat\private_out.bat call bat\private_out.bat TEST 32

:DONE
endlocal
