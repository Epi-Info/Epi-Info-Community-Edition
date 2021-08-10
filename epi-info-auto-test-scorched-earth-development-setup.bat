CLS
@ECHO OFF
COLOR 0A

ECHO :: ===============================================================
ECHO :: SET LOCAL VARIABLES
ECHO :: ===============================================================
SETLOCAL ENABLEDELAYEDEXPANSION
SET batchRootDirectory=%CD%
SET projectFoldername=auto-test
SET ei7=%batchRootDirectory%\projectFoldername
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
ECHO ei7: %ei7%
:: ===============================================================

ECHO :: ===============================================================
ECHO :: DELETE AUTO TEST FOLDER
ECHO :: ===============================================================
IF %QUIET%==TRUE GOTO :DELETEEPIINFOFOLDER
:ASK_SKIP_DELETE
SET /P d=DELETE AUTO TEST FOLDER [Y/N]?
IF /I "%d%" EQU "Y" GOTO :DELETEEPIINFOFOLDER
IF /I "%d%" EQU "N" GOTO :SKIP_DELETE
GOTO :ASK_SKIP_DELETE
:DELETEEPIINFOFOLDER
IF EXIST ".\auto-test" (
    ECHO.
    ECHO Deleting auto-test directory
) 
IF EXIST ".\auto-test" (
    RMDIR /S /Q .\auto-test
) 
IF EXIST ".\auto-test" (
    COLOR 0A
    ECHO auto-test - still there - try restartExplorer.bat
    EXIT /B
) ELSE (
    COLOR
    ECHO auto-test - gone
)
:SKIP_DELETE
ECHO :: ===============================================================
ECHO :: GET SOURCE - GIT CLONE AUTO TEST REPO
ECHO :: ===============================================================
IF %QUIET%==TRUE GOTO:GET_SOURCE
IF /I "%d%" EQU "Y" GOTO :GET_SOURCE
:ASK_GET_SOURCE
SET /P o=OVERWRITE (GET) AUTO TEST FROM GITHUB [Y/N]?
IF /I "%o%" EQU "Y" GOTO :GET_SOURCE
IF /I "%o%" EQU "N" GOTO :SKIP_GET_SOURCE
GOTO :ASK_GET_SOURCE
:GET_SOURCE
@ECHO ON
git clone https://github.com/cdc-dpbrown/auto-test.git
@ECHO OFF

:: === CHECK OUT SPECIFIC COMMIT ===
::CD %ei7%
::git checkout e2cd415b3308d1612ec368a06eec82b5d6512041
::git checkout fc96dd557a385d37e048e904ed17aea76c663ec8
::PAUSE
:SKIP_GET_SOURCE

ECHO :: ===============================================================
ECHO :: OPEN WINDOWS EXPLORER IN AUTO TEST 7 DIRECTORY
ECHO :: ===============================================================
@ECHO ON
:: EXPLORER %ei7%
@ECHO OFF
:: ===============================================================

ECHO :: ===============================================================
ECHO :: OPEN SOLUTION IN VISUAL STUDIO
ECHO :: ===============================================================
@ECHO ON
CD %batchRootDirectory%
CALL nuget restore %ei7%"\AUTO TEST 7.sln"
CD %ei7%
CALL "AUTO TEST 7.sln"
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