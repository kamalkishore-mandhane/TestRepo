@echo OFF

@echo Generate %1 %2 %3 %4 %5
@echo.

Bat\ResExtract%5 %1\%2\%3.resources.dll

copy %1\%3.g.resources %1\%2\%3.g.%2.resources
if exist %1\%2\%3.template.dll (del %1\%2\%3.template.dll)
ren %1\%2\%3.resources.dll %3.template.dll

@echo /out:%1\%2\%3.resources.dll >%1\%2\%3.response.txt
@echo /c:%2 >>%1\%2\%3.response.txt
@echo /keyf:%4.snk >>%1\%2\%3.response.txt
@echo /t:lib >>%1\%2\%3.response.txt
@echo /template:%1\%2\%3.template.dll >>%1\%2\%3.response.txt

for /R %1 %%I in (%3.*.%2.resources) do (
  @echo /embed:%1\%2\%%~nI.resources >>%1\%2\%3.response.txt
)

for /R %1 %%I in (%4.*.%2.resources) do (
  @echo /embed:%1\%2\%%~nI.resources >>%1\%2\%3.response.txt
)

al.exe @%1\%2\%3.response.txt

@echo.
