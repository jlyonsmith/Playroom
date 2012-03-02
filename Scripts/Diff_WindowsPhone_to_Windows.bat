call setg diff
setlocal
set TOYBOX=..\ToyBox
kdiff3 %TOYBOX%\ToyBox[WindowsPhone].csproj %TOYBOX%\ToyBox[Windows].csproj
endlocal
