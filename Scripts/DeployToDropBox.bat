@echo off
setlocal
call SetSlnRoot.bat
set Config=%1
if "%Config%"=="" set Config=Release
set TargetDir=%HomePath%\Documents\My Dropbox\Jamoki\Software\Playroom
md "%TargetDir%"
call %SolutionRoot%\Scripts\XCopyFiles.bat
endlocal
