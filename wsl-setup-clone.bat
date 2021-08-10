@ECHO OFF
COLOR

IF EXIST ".\eriks-wsl-setup" (
    ECHO.
    ECHO Deleting eriks-wsl-setup directory
) 

IF EXIST ".\eriks-wsl-setup" (
    RMDIR /S /Q .\eriks-wsl-setup
) 

IF EXIST ".\eriks-wsl-setup" (
    COLOR 0A
    ECHO eriks-wsl-setup - still there - try restartExplorer.bat
    PAUSE
) ELSE (
    COLOR
    ECHO eriks-wsl-setup - gone
)

ECHO ON
COLOR 

git clone https://github.com/erik1066/windows-wsl-ubuntu-setup.git eriks-wsl-setup
CD eriks-wsl-setup

COLOR 
