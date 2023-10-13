param (
    [Parameter(Mandatory=$true)]
    [string]$OutputDir
)

New-Item -ItemType directory -Path ${OutputDir}/docs/doxygen -ErrorAction Ignore
New-Item -ItemType directory -Path ${OutputDir}/docs/tagfiles -ErrorAction Ignore
New-Item -ItemType directory -Path ${OutputDir}/docs/output -ErrorAction Ignore
Invoke-WebRequest -Uri https://gitlab.com/api/v4/projects/4207231/packages/generic/graphviz-releases/9.0.0/windows_10_msbuild_Release_graphviz-9.0.0-win32.zip -OutFile "${OutputDir}/Graphviz.zip"
Expand-Archive -Path "${OutputDir}/Graphviz.zip" -DestinationPath ${OutputDir}
Invoke-WebRequest -Uri https://github.com/plantuml/plantuml/releases/download/v1.2023.11/plantuml-1.2023.11.jar -OutFile "${OutputDir}/plantuml.jar"
pip install doxysphinx --user
pip install sphinxcontrib-doxylink --user
pip install sphinxcontrib-plantuml --user
(Get-Content "./tooling/conf.py.template") | ForEach-Object {
    $_ -replace "${DOC_OUTPUT_DIR}", "${OutputDir}"
} | Set-Content "${OutputDir}/docs/conf.py"
doxygen ./documentation/Doxyfile
python -m doxysphinx build "${OutputDir}/docs" "${OutputDir}/docs/output" "./documentation/Doxyfile"
