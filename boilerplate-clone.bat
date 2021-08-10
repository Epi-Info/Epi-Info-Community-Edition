@ECHO OFF
COLOR

IF EXIST ".\epi-info-boilerplate" (
    ECHO.
    ECHO Deleting epi-info-boilerplate directory
) 

IF EXIST ".\epi-info-boilerplate" (
    RMDIR /S /Q .\epi-info-boilerplate
) 

IF EXIST ".\epi-info-boilerplate" (
    COLOR 0A
    ECHO epi-info-boilerplate - still there - try restartExplorer.bat
    PAUSE
) ELSE (
    COLOR
    ECHO epi-info-boilerplate - gone
)

ECHO ON
COLOR 

git clone https://github.com/cdc-dpbrown/epi-info-boilerplate.git epi-info-boilerplate
CD epi-info-boilerplate

dotnet restore
dotnet build

"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" http://localhost:5000/

dotnet run
