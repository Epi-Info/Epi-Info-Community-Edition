@ECHO OFF
COLOR

IF EXIST ".\epi-info-maps" (
    ECHO.
    ECHO Deleting epi-info-maps directory
) 

IF EXIST ".\epi-info-maps" (
    RMDIR /S /Q .\epi-info-maps
) 

IF EXIST ".\epi-info-maps" (
    COLOR 0A
    ECHO epi-info-maps - still there - try restartExplorer.bat
    PAUSE
) ELSE (
    COLOR
    ECHO epi-info-maps - gone
)

ECHO ON
COLOR 

REM git clone --depth 1 --single-branch --branch master https://github.com/cdc-dpbrown/epi-info-maps.git epi-info-maps
git clone https://github.com/cdc-dpbrown/epi-info-maps.git epi-info-maps
cd epi-info-maps

COLOR 

REM [comment] Added OpenLayers via react-openlayers wrapper
::git reset --hard 5bac8b278467a63d6c085387b8ae8cb7e36563c5

REM [comment] initial check-in
::git reset --hard 3c632651022ffe4f43f2b1b4c12a9a5cc2c7e828

IF EXIST ".\package-lock.json" (
    ECHO.
    ECHO Deleting package-lock.json
    DEL /Q .\package-lock.json
) 

CALL yarn
:: CALL yarn list --pattern "epi-"
CALL yarn dev