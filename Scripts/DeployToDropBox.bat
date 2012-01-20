setlocal
set DROPBOX_DIR=C:\Users\john\Documents\My Dropbox\Jamoki\Software\Playroom
set PLAYROOM_DIR=..\Playroom\bin\Release\
set PRISM_DIR=..\Prism\bin\Release\
set PINATA_DIR=..\Pinata\bin\Release\
set PINBOARD_DIR=..\Pinboard\bin\Release\
xcopy /d /y "%PLAYROOM_DIR%Playroom.dll" "%DROPBOX_DIR%"
xcopy /d /y "%PLAYROOM_DIR%ToolBelt.1.5.dll" "%DROPBOX_DIR%"
xcopy /d /y "%PLAYROOM_DIR%Playroom.targets" "%DROPBOX_DIR%"
xcopy /d /y "%PINBOARD_DIR%Pinboard.exe" "%DROPBOX_DIR%"
xcopy /d /y "%PRISM_DIR%Prism.exe" "%DROPBOX_DIR%"
xcopy /d /y "%PINATA_DIR%Pinata.exe" "%DROPBOX_DIR%"
endlocal
