@echo off
setlocal
call SetSlnRoot.bat
set Config=%1
if "%Config%"=="" set Config=Release
set TargetDir=c:\program files (x86)\MSBuild\Playroom
md "%TargetDir%"
call XCopyFiles.bat
set TargetDir=c:\program files\MSBuild\Playroom
md "%TargetDir%"
call XCopyFiles.bat
endlocal