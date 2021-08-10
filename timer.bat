@ECHO OFF
:START
CLS
DIR
git status
COLOR 02
ECHO :: ===================================
ECHO :: SEVEN POINT FIVE MINUTE TIMER
ECHO :: ===================================
TIMEOUT /T 450
CLS
COLOR 0A
DIR
git status
PAUSE
GOTO :START