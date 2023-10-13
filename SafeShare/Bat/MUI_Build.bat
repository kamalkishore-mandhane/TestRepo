@echo OFF

@echo Generate %1 %2 %3 %4
@echo.

Bat\ResExtract%4 %1\%2\%3%4.resources.dll

copy %1\%3%4.g.resources %1\%2\%3%4.g.%2.resources
if exist %1\%2\%3%4.template.dll (del %1\%2\%3%4.template.dll)
ren %1\%2\%3%4.resources.dll %3%4.template.dll

@echo /out:%1\%2\%3%4.resources.dll >%1\%2\%3%4.response.txt
@echo /c:%2 >>%1\%2\%3%4.response.txt
@echo /keyf:%3.snk >>%1\%2\%3%4.response.txt
@echo /t:lib >>%1\%2\%3%4.response.txt
@echo /template:%1\%2\%3%4.template.dll >>%1\%2\%3%4.response.txt

for /R %1 %%I in (%3%4.*.%2.resources) do (
  @echo /embed:%1\%2\%%~nI.resources >>%1\%2\%3%4.response.txt
)

for /R %1 %%I in (%3.*.%2.resources) do (
  @echo /embed:%1\%2\%%~nI.resources >>%1\%2\%3%4.response.txt
)

al.exe @%1\%2\%3%4.response.txt

@echo.
