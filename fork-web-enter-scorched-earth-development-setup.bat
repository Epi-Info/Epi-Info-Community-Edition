CLS
@ECHO OFF
COLOR 0A

ECHO :: ===============================================================
ECHO :: SET LOCAL VARIABLES
ECHO :: ===============================================================
SETLOCAL ENABLEDELAYEDEXPANSION
SET buildEXE="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
SET batchRootDirectory=%CD%
SET webEnter=%batchRootDirectory%\Epi-Info-Cloud-Data-Capture
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
ECHO webEnter: %webEnter%
:: ===============================================================

ECHO :: ===============================================================
ECHO :: DELETE WEB SURVEY FOLDER
ECHO :: ===============================================================
IF %QUIET%==TRUE GOTO :DELETEEPIINFOFOLDER
:ASK_SKIP_DELETE
SET /P d=DELETE WEB SURVEY FOLDER [Y/N]?
IF /I "%d%" EQU "Y" GOTO :DELETEEPIINFOFOLDER
IF /I "%d%" EQU "N" GOTO :SKIP_DELETE
GOTO :ASK_SKIP_DELETE
:DELETEEPIINFOFOLDER
IF EXIST ".\Epi-Info-Cloud-Data-Capture" (
    ECHO.
    ECHO Deleting Epi-Info-Cloud-Data-Capture directory
) 
IF EXIST ".\Epi-Info-Cloud-Data-Capture" (
    RMDIR /S /Q .\Epi-Info-Cloud-Data-Capture
) 
IF EXIST ".\Epi-Info-Cloud-Data-Capture" (
    COLOR 0A
    ECHO Epi-Info-Cloud-Data-Capture - still there - try restartExplorer.bat
    EXIT /B
) ELSE (
    COLOR
    ECHO Epi-Info-Cloud-Data-Capture - gone
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
git clone https://github.com/cdc-dpbrown/Epi-Info-Cloud-Data-Capture.git
@ECHO OFF
::CD %webEnter%
::git reset --hard ba6476afb4d300614ac58c66ec84dc91d83bda74
::git reset --hard ba6476afb4d300614ac58c66ec84dc91d83bda74
:SKIP_GET_SOURCE
:: ===============================================================

ECHO :: ===============================================================
ECHO :: OPEN WINDOWS EXPLORER IN WEB SURVEY DIRECTORY
ECHO :: ===============================================================
@ECHO ON
::EXPLORER %webEnter%
@ECHO OFF
:: ===============================================================

ECHO :: ===============================================================
ECHO :: OPEN SOLUTION IN VISUAL STUDIO
ECHO :: ===============================================================
@ECHO ON
CD %batchRootDirectory%

GOTO :EOF

CALL nuget restore %webEnter%"\Epi Info Web Enter.sln"
CD %webEnter%
CALL %buildEXE% "Epi Info Web Enter.sln" -t:restore 
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.exe" "Epi Info Web Enter.sln"
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