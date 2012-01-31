if "%SolutionRoot%"=="" goto :eof
if "%Config%"=="" goto :eof
if "%TargetDir%"=="" goto :eof
xcopy /dy "%SolutionRoot%\Playroom\bin\%Config%\Playroom.dll" "%TargetDir%"
xcopy /dy "%SolutionRoot%\Playroom\bin\%Config%\ToolBelt.dll" "%TargetDir%"
xcopy /dy "%SolutionRoot%\Playroom\bin\%Config%\Playroom.targets" "%TargetDir%"
xcopy /dy "%SolutionRoot%\Pinboard\bin\%Config%\Pinboard.exe" "%TargetDir%"
xcopy /dy "%SolutionRoot%\Pinata\bin\%Config%\Pinata.exe" "%TargetDir%"
