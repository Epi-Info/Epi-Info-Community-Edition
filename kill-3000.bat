@ECHO OFF
COLOR

netstat -ano | findstr :3000

ECHO %ERRORLEVEL%

IF %ERRORLEVEL% EQU 1 (
    ECHO NOT FOUND: NOT RUNNING
    GOTO:EOF
)

ECHO.
ECHO Enter the process ID found above.
SET /P proID=
CALL taskkill /PID %proID% /F

GOTO:EOF

