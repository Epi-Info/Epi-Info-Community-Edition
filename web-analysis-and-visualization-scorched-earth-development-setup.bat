CLS
@ECHO OFF
COLOR 0A

ECHO :: ===============================================================
ECHO :: SET LOCAL VARIABLES
ECHO :: ===============================================================
SETLOCAL ENABLEDELAYEDEXPANSION
SET batchRootDirectory=%CD%
SET ewavFolder=%batchRootDirectory%\EWAV
SET KEY_QUIET=Q
SET KEY_HELP=/?
SET ARGS=%1
IF "%ARGS%" EQU "" SET ARGS=1
SET QUIET=FALSE
SET HELP=FALSE
IF NOT "x!ARGS:%KEY_QUIET%=!"=="x%ARGS%" SET QUIET=TRUE
IF NOT "x!ARGS:%KEY_HELP%=!"=="x%ARGS%" GOTO :HELP
ECHO ARGS=%ARGS%
ECHO QUIET: %QUIET%
ECHO HELP: %HELP%
ECHO batchRootDirectory: %batchRootDirectory%
ECHO ewavFolder: %ewavFolder%
:: ===============================================================

ECHO :: ===============================================================
ECHO :: DELETE EWAV FOLDER
ECHO :: ===============================================================
IF %QUIET%==TRUE GOTO :DELETEEPIINFOFOLDER
:ASK_SKIP_DELETE
SET /P d=DELETE WEB SURVEY FOLDER [Y/N]?
IF /I "%d%" EQU "Y" GOTO :DELETEEPIINFOFOLDER
IF /I "%d%" EQU "N" GOTO :SKIP_DELETE
GOTO :ASK_SKIP_DELETE
:DELETEEPIINFOFOLDER
IF EXIST ".\EWAV" (
    ECHO.
    ECHO Deleting EWAV directory
) 
IF EXIST ".\EWAV" (
    RMDIR /S /Q .\EWAV
) 
IF EXIST ".\EWAV" (
    COLOR 0A
    ECHO EWAV - still there - try restartExplorer.bat
    EXIT /B
) ELSE (
    COLOR
    ECHO EWAV - gone
)
:SKIP_DELETE
ECHO :: ===============================================================
ECHO :: GET SOURCE - GIT CLONE WEB SURVEY REPO
ECHO :: ===============================================================
IF %QUIET%==TRUE GOTO:GET_SOURCE
IF /I "%d%" EQU "Y" GOTO :GET_SOURCE
:ASK_GET_SOURCE
SET /P o=OVERWRITE (GET) WEB SURVEY FROM GITHUB [Y/N]?
IF /I "%o%" EQU "Y" GOTO :GET_SOURCE
IF /I "%o%" EQU "N" GOTO :SKIP_GET_SOURCE
GOTO :ASK_GET_SOURCE
:GET_SOURCE
@ECHO ON
git clone https://github.com/cdc-dpbrown/EWAV.git
@ECHO OFF
::CD %ewavFolder%
::git reset --hard c5baca6a6c08f2168bc28d00f7db0e2ce104fd24
:SKIP_GET_SOURCE
:: ===============================================================

ECHO :: ===============================================================
ECHO :: OPEN WINDOWS EXPLORER IN WEB SURVEY DIRECTORY
ECHO :: ===============================================================
@ECHO ON
::EXPLORER %ewavFolder%
@ECHO OFF
:: ===============================================================

ECHO :: ===============================================================
ECHO :: OPEN SOLUTION IN VISUAL STUDIO
ECHO :: ===============================================================
@ECHO ON
CD %batchRootDirectory%
CALL nuget restore %ewavFolder%"\Ewav.sln"
CD %ewavFolder%
PAUSE
::"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.exe" "Ewav.sln"
"Ewav.sln"
@ECHO OFF
:: ===============================================================

ENDLOCAL
GOTO :EOF

:HELP
ECHO :: ===============================================================
ECHO :: HELP
ECHO :: ===============================================================
ECHO Q   Quiet mode
GOTO :EOF
:: ===============================================================

:EOF