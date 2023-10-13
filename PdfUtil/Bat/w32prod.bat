@echo OFF

setlocal
call %_LOCAL_SETENV_% ATL BOOST
devenv "PdfUtil.sln" /rebuild "Release|x86" 

xcopy bin\Release\x86\*.exe w32prod\ /s /h /d /y
xcopy bin\Release\x86\*.dll w32prod\ /s /h /d /y
xcopy bin\Release\x86\*.xml w32prod\ /s /h /d /y
xcopy bin\Release\x86\*.pdb w32prod\ /s /h /d /y
xcopy bin\Release\x86\*.config w32prod\ /s /h /d /y

call bat\MUI.bat w32prod PdfUtil 32
@echo WzBuild Error Level is %ERRORLEVEL%
if errorlevel 1 goto DONE

call bat\MUI.bat w32prod StartupPaneLib
@echo WzBuild Error Level is %ERRORLEVEL%
if errorlevel 1 goto DONE

if exist bat\private_out.bat call bat\private_out.bat PROD 32

:DONE
endlocal
