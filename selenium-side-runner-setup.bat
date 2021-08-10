CLS
@ECHO OFF
COLOR

ECHO :: ===============================================================
ECHO :: SET LOCAL VARIABLES
ECHO :: ===============================================================
SETLOCAL ENABLEDELAYEDEXPANSION
SET batchRootDirectory=%CD%
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
ECHO BATCH ROOT DIRECTORY: %batchRootDirectory%

ECHO.
WHERE NPM
IF %ERRORLEVEL% EQU 0 (
     ECHO NPM: GOOD
     CALL npm -v
) ELSE (
     ECHO NPM: NO
)

ECHO.
WHERE NODE
IF %ERRORLEVEL% EQU 0 (
     ECHO NODEJS: GOOD
     CALL node -v
) ELSE (
     ECHO NODEJS: NO
)

ENDLOCAL

:: STOP HERE IF NODE AND NPM ARE NOT INSTALLED
:: https://nodejs.org/en/download/

::GOTO:SKIP_GLOBAL
:: SELENIUM SIDE RUNNER
CALL npm install -g selenium-side-runner
:: CHROME
CALL npm install -g chromedriver
:: EDGE
CALL npm install -g edgedriver
:: FIREFOX
CALL npm install -g geckodriver
:: IE
CALL npm install -g iedriver
:::SKIP_GLOBAL

:: SELENIUM SIDE RUNNER
::CALL npm install selenium-side-runner
:: CHROME
::CALL npm install chromedriver
:: EDGE
::CALL npm install edgedriver
:: FIREFOX
::CALL npm install geckodriver
:: IE
::CALL npm install iedriver

:: You'll also need to have the browser installed on your machine.



CALL npm view selenium-side-runner

CALL npm view chromedriver
WHERE chromedriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\chromedriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\chromedriver\

CALL npm view edgedriver
WHERE edgedriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\edgedriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\edgedriver\

CALL npm view geckodriver
WHERE geckodriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\geckodriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\geckodriver\

CALL npm view iedriver
WHERE iedriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\iedriver
::SET PATH=%PATH%;C:\Users\ita3\AppData\Roaming\npm\iedriver\

GOTO :EOF

:HELP
ECHO :: ===============================================================
ECHO :: HELP
ECHO :: ===============================================================
ECHO start chrome https://www.seleniumhq.org/selenium-ide/docs/en/introduction/command-line-runner/
GOTO :EOF

:EOF
