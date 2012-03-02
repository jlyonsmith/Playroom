call setg diff
setlocal
set TOYBOX=..\ToyBox
kdiff3 %TOYBOX%\ToyBox[Windows].csproj %TOYBOX%\ToyBox[WindowsPhone].csproj
endlocal
