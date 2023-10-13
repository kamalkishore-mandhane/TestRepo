@echo OFF

set argC=0
for %%x in (%*) do Set /A argC+=1

if %argC%==3 (goto THREEARGS)
if %argC%==5 (goto FIVEARGS)

:THREEARGS
@echo Generate %1 %2 %3
@echo.
Bat\ResExtract %1\%2\%3.resources.dll
goto NEXT

:FIVEARGS
@echo Generate %1 %2 %3 %4 %5
@echo.
Bat\ResExtract%5 %1\%2\%3.resources.dll
goto NEXT

:NEXT
copy %1\%3.g.resources %1\%2\%3.g.%2.resources
if exist %1\%2\%3.template.dll (del %1\%2\%3.template.dll)
ren %1\%2\%3.resources.dll %3.template.dll

@echo /out:%1\%2\%3.resources.dll >%1\%2\%3.response.txt
@echo /c:%2 >>%1\%2\%3.response.txt
if %argC%==3 echo /keyf:%3\%3.snk >>%1\%2\%3.response.txt
if %argC%==5 echo /keyf:%4.snk >>%1\%2\%3.response.txt
@echo /t:lib >>%1\%2\%3.response.txt
@echo /template:%1\%2\%3.template.dll >>%1\%2\%3.response.txt

for /R %1 %%I in (%3.*.%2.resources) do (
  @echo /embed:%1\%2\%%~nI.resources >>%1\%2\%3.response.txt
)

if %argC%==3 (goto ALCODE)

for /R %1 %%I in (%4.*.%2.resources) do (
  @echo /embed:%1\%2\%%~nI.resources >>%1\%2\%3.response.txt
)

:ALCODE
al.exe @%1\%2\%3.response.txt

@echo.
