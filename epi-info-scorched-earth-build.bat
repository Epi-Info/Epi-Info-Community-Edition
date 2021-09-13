CLS
::@ECHO OFF
COLOR 0A

ECHO.
ECHO :: ===============================================================
ECHO :: SET LOCAL VARIABLES
ECHO :: ===============================================================
ECHO.
SETLOCAL ENABLEDELAYEDEXPANSION
:: Use \Bin\MSBuild.exe, which can be run on 64-bit machine and as x86 on a 32-bit.
SET buildEXE="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
SET batchRootDirectory=%CD%
SET requiredFilesDirectory=C:\EpiInfo7ReleaseBuildFiles
SET ei7=%batchRootDirectory%\Epi-Info-Community-Edition
SET KEY_QUIET=Q
SET KEEP_KEEP_RELEASE_KEYS=K
SET KEY_HELP=/?
SET ARGS=%1
IF "%ARGS%" EQU "" SET ARGS=1
SET QUIET=FALSE
SET KEEP_RELEASE_KEYS=FALSE
SET HELP=FALSE
IF NOT "x!ARGS:%KEY_QUIET%=!"=="x%ARGS%" SET QUIET=TRUE
IF NOT "x!ARGS:%KEEP_KEEP_RELEASE_KEYS%=!"=="x%ARGS%" SET KEEP_RELEASE_KEYS=TRUE
IF NOT "x!ARGS:%KEY_HELP%=!"=="x%ARGS%" GOTO :HELP
ECHO MSBUILD.EXE PATH: %buildEXE%
ECHO ARGS=%ARGS%
ECHO QUIET: %QUIET%
ECHO KEEP_RELEASE_KEYS: %KEEP_RELEASE_KEYS%
ECHO HELP: %HELP%
ECHO batchRootDirectory: %batchRootDirectory%
ECHO requiredFilesDirectory: %requiredFilesDirectory%
ECHO ei7: %ei7%

ECHO.
ECHO :: ===============================================================
ECHO :: DELETE EPI INFO FOLDER
ECHO :: ===============================================================
ECHO.
IF %QUIET%==TRUE GOTO :DELETEEPIINFOFOLDER
:ASK_SKIP_DELETE
SET /P d=DELETE EPI INFO FOLDER [Y/N]?
IF /I "%d%" EQU "Y" GOTO :DELETEEPIINFOFOLDER
IF /I "%d%" EQU "N" GOTO :SKIP_DELETE
GOTO :ASK_SKIP_DELETE
:DELETEEPIINFOFOLDER
IF EXIST ".\Epi-Info-Community-Edition" (
    ECHO Deleting Epi-Info-Community-Edition directory
) 
IF EXIST ".\Epi-Info-Community-Edition" (
    RMDIR /S /Q .\Epi-Info-Community-Edition
) 
IF EXIST ".\Epi-Info-Community-Edition" (
    COLOR 0A
    ECHO Epi-Info-Community-Edition - still there - try restartExplorer.bat
    EXIT /B
) ELSE (
    COLOR
    ECHO Epi-Info-Community-Edition - gone
)
:SKIP_DELETE

ECHO.
ECHO :: ===============================================================
ECHO :: GET SOURCE - GIT CLONE EPI INFO REPO
ECHO :: ===============================================================
IF %QUIET%==TRUE GOTO:GET_SOURCE
IF /I "%d%" EQU "Y" GOTO :GET_SOURCE
:ASK_GET_SOURCE
SET /P o=OVERWRITE (GET) EPI INFO FROM GITHUB [Y/N]?
IF /I "%o%" EQU "Y" GOTO :GET_SOURCE
IF /I "%o%" EQU "N" GOTO :SKIP_GET_SOURCE
GOTO :ASK_GET_SOURCE
:GET_SOURCE
@ECHO ON
git clone https://github.com/Epi-Info/Epi-Info-Community-Edition.git

:: === CHECK OUT SPECIFIC COMMIT ===
::CD %ei7%
::git checkout e2cd415b3308d1612ec368a06eec82b5d6512041
::git checkout fc96dd557a385d37e048e904ed17aea76c663ec8
::PAUSE

@ECHO OFF
:SKIP_GET_SOURCE

IF %QUIET%==TRUE GOTO:SKIP_UPDATE_VERSION
ECHO.
ECHO :: ===============================================================
ECHO :: UPDATE VERSION
ECHO :: ===============================================================
ECHO .
ECHO :: SolutionInfo.cs
ECHO :: [assembly: AssemblyVersion("7.2.*")]
ECHO :: [assembly: AssemblyFileVersion("7.2.4.V")]
ECHO :: [assembly: AssemblyInformationalVersion("7.2.4.V")]
ECHO :: [assembly: SatelliteContractVersion("7.0.0.0")]
ECHO :: [assembly: Epi.AssemblyReleaseDateAttribute("MM/DD/YYYY")]
ECHO .
ECHO :: AssemblyInfo.cs
ECHO :: [assembly: AssemblyVersion("7.2.4.V")]
ECHO :: [assembly: AssemblyFileVersion("7.2.4.V")]
ECHO .
ECHO :: AssemblyInfo.vb
ECHO :: Assembly: AssemblyVersion("7.2.4.V")
ECHO :: Assembly: AssemblyFileVersion("7.2.4.V")
ECHO .
ECHO An instance of Visual Studio Code will open and you can make the changes there.
ECHO .
PAUSE
CALL code -n %ei7%\SolutionInfo.cs %ei7%\EpiInfoPlugin\Properties\AssemblyInfo.cs %ei7%"\StatisticsRepository\My Project\AssemblyInfo.vb"
SET /P commit=COMMIT CHANGES TO GITHUB [Y/N]?
:ASK_COMMIT_CHANGES
IF /I "%commit%" EQU "Y" GOTO :COMMIT_CHANGES
IF /I "%commit%" EQU "N" GOTO :SKIP_COMMIT_CHANGES
GOTO :ASK_COMMIT_CHANGES
GOTO :SKIP_COMMIT_CHANGES
:COMMIT_CHANGES
CHDIR
ECHO.
ECHO :: ===============================================================
ECHO :: UPDATE VERSION
ECHO :: [BUILD] 7.2.4.V M/D/20YY 
ECHO :: ===============================================================
ECHO .
SET /P V=Minor Version(V): 
SET /P M=Month(M): 
SET /P D=Date(D): 
SET /P Y=Year(YY):
SET commit_message=[BUILD] 7.2.4.%V% %M%/%D%/20%Y%
ECHO %commit_message%
ECHO .
@ECHO ON
CD %ei7%
git status
git add SolutionInfo.cs .\EpiInfoPlugin\Properties\AssemblyInfo.cs ".\StatisticsRepository\My Project\AssemblyInfo.vb"
git status
git commit -m "%commit_message%"
git status
git pull --rebase
@ECHO OFF

SET /P commitCheck=ARE YOU SURE YOU WANT TO COMMIT [YES/N]?
:ASK_ARE_YOU_SURE_COMMIT_CHANGES
IF /I "%commitCheck%" EQU "YES" GOTO :ARE_YOU_SURE_COMMIT_CHANGES
IF /I "%commitCheck%" EQU "N" GOTO :SKIP_ARE_YOU_SURE_COMMIT_CHANGES
GOTO :ASK_ARE_YOU_SURE_COMMIT_CHANGES
:ARE_YOU_SURE_COMMIT_CHANGES
@ECHO ON
git push
@ECHO OFF

:SKIP_ARE_YOU_SURE_COMMIT_CHANGES
CD ..
PAUSE

:SKIP_COMMIT_CHANGES
:SKIP_UPDATE_VERSION

ECHO.
ECHO :: ===============================================================
ECHO :: COPY KEYS
ECHO :: REPLACE THE COMPONENT ART LICENSE
ECHO :: ===============================================================
ECHO.
ECHO %requiredFilesDirectory%\Configuration_Static.cs
COPY /Y %requiredFilesDirectory%\Configuration_Static.cs %ei7%\Epi.Core\Configuration_Static.cs
COPY /Y %requiredFilesDirectory%\ComponentArt.Win.DataVisualization.lic %ei7%\Epi.Windows.AnalysisDashboard\ComponentArt.Win.DataVisualization.lic
COPY /Y %requiredFilesDirectory%\ComponentArt.Win.DataVisualization.lic %ei7%\Epi.Windows.Enter\ComponentArt.Win.DataVisualization.lic
COPY /Y %requiredFilesDirectory%\ComponentArt.Win.DataVisualization.lic %ei7%\EpiDashboard\ComponentArt.Win.DataVisualization.lic
IF %QUIET%==TRUE GOTO:SKIP_VERIFY_KEYS_COPY
::ECHO OPEN IN CODE TO VERIFY THE COMPONENT LICENSE HAS CHANGED
CALL code -n %ei7%
PAUSE
:SKIP_VERIFY_KEYS_COPY


ECHO.
ECHO :: ===============================================================
ECHO :: COPY DLLS
ECHO :: ===============================================================
ECHO.
CD %ei7%
IF NOT EXIST "build" (
    COLOR
    MKDIR build
)
IF NOT EXIST ".\build\release" (
    COLOR
    CD build
    MKDIR release
)
COPY /Y %requiredFilesDirectory%\dll\Epi.Data.PostgreSQL.dll %ei7%\build\release\Epi.Data.PostgreSQL.dll
COPY /Y %requiredFilesDirectory%\dll\FipsCrypto.dll %ei7%\build\release\FipsCrypto.dll
COPY /Y %requiredFilesDirectory%\dll\Interop.PortableDeviceApiLib.dll %ei7%\build\release\Interop.PortableDeviceApiLib.dll
COPY /Y %requiredFilesDirectory%\dll\Interop.PortableDeviceTypesLib.dll %ei7%\build\release\Interop.PortableDeviceTypesLib.dll
COPY /Y %requiredFilesDirectory%\dll\Mono.Security.dll %ei7%\build\release\Mono.Security.dll
COPY /Y %requiredFilesDirectory%\dll\Npgsql.dll %ei7%\build\release\Npgsql.dll


ECHO.
ECHO :: ===============================================================
ECHO :: BUILD
ECHO :: ===============================================================
ECHO.
CD %batchRootDirectory%
CALL nuget restore %ei7%"\Epi Info 7.sln"
ECHO.
ECHO.
ECHO     [ STARTING BUILD ] PLEASE WAIT ... ( ~30 SECONDS )
ECHO.
ECHO.
CALL %buildEXE% %ei7%"\Epi Info 7.sln" /m /p:Configuration=Release /p:Platform=x86 /clp:Summary=true;ErrorsOnly

:: where /r c:\ MSBuild.exe

IF NOT %QUIET%==TRUE PAUSE

ECHO.
ECHO :: ===============================================================
ECHO :: COPY LAUNCH EPI INFO EXECUTABLE 
ECHO :: ===============================================================
@ECHO ON
COPY /Y %requiredFilesDirectory%"\Launch Epi Info 7.exe" %ei7%"\build\Launch Epi Info 7.exe"
@ECHO OFF

IF %KEEP_RELEASE_KEYS%==TRUE GOTO:SKIP_UNDO_RELEASE_KEYS_AND_LICENSE
ECHO.
ECHO :: ===============================================================
ECHO :: GIT CHECKOUT ALL - (UNDO KEYS AND LICENSE FILES)
ECHO :: ===============================================================
@ECHO ON
CD %ei7%
git checkout -- *
git status
@ECHO OFF
:SKIP_UNDO_RELEASE_KEYS_AND_LICENSE
IF NOT %QUIET%==TRUE PAUSE

ECHO.
ECHO :: ===============================================================
ECHO :: COPY PROJECTS (NOT NEEDED IF BUILD CONFIG RIGHT)
ECHO :: ===============================================================
@ECHO ON
::XCOPY %ei7%\Epi.Core\Projects %ei7%\build\release\Projects /I /S /V /F /Y
::
:: /S   Copies directories and subdirectories except empty ones.
:: /V   Verifies the size of each new file.
:: /I   If destination does not exist and copying more than one file,
::      assumes that destination must be a directory.
::
@ECHO OFF

ECHO.
ECHO :: ===============================================================
ECHO :: PRUNE FILES
ECHO :: ===============================================================
@ECHO ON
RMDIR /S /Q  %ei7%\Build\release\app.publish
RMDIR /S /Q  %ei7%\Build\release\Configuration
RMDIR /S /Q  %ei7%\Build\release\Logs
RMDIR /S /Q  %ei7%\Build\release\TestCases
RMDIR /S /Q  %ei7%\Build\release\Templates\Projects
RMDIR /S /Q  %ei7%\Build\release\de
RMDIR /S /Q  %ei7%\Build\release\es
RMDIR /S /Q  %ei7%\Build\release\fr
RMDIR /S /Q  %ei7%\Build\release\it
RMDIR /S /Q  %ei7%\Build\release\ja
RMDIR /S /Q  %ei7%\Build\release\ko
RMDIR /S /Q  %ei7%\Build\release\ru
RMDIR /S /Q  %ei7%\Build\release\zh-Hans
RMDIR /S /Q  %ei7%\Build\release\zh-Hant
DEL /Q %ei7%\Build\release\Output\*.html
DEL /Q %ei7%\Build\release\*.pdb
@ECHO OFF

ECHO.
ECHO :: ===============================================================
ECHO :: COPY RELEASE FOLDER TO EPI INFO 7
ECHO :: ===============================================================
ECHO.
XCOPY %ei7%\Build\release %ei7%\Build\"Epi Info 7" /I /E /Q

ECHO.
ECHO :: ===============================================================
ECHO :: COPY EPI INFO 7 TO Epi Info 7@2019-03-24@16-34-32
ECHO :: ===============================================================
ECHO.
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"
set "fullstamp=@%YYYY%-%MM%-%DD%@%HH%-%Min%-%Sec%"
echo fullstamp: "%fullstamp%
SET newName=%ei7%\Build\"Epi Info 7"%fullstamp%
XCOPY %ei7%\Build\"Epi Info 7" %newName% /I /E /Q

ECHO.
ECHO :: ===============================================================
ECHO :: OPEN EPI MENU
ECHO :: ===============================================================
@ECHO ON
CD %ei7%\build\release
START %ei7%\build\release\Menu.exe
ECHO %CD%
@ECHO OFF

ECHO.
ECHO :: ===============================================================
ECHO :: OPEN WINDOWS EXPLORER IN BUILD DIRECTORY
ECHO :: ===============================================================
@ECHO ON
EXPLORER %ei7%\build
@ECHO OFF


ECHO.
ECHO :: ===============================================================
ECHO :: WRITE COMMIT REPORT AND VIEW IN CODE
ECHO :: ===============================================================
@ECHO ON
DEL commitReport.code
git.exe log -100 --pretty=format:%%s > commitReport.code
code commitReport.code
@ECHO OFF


ECHO.
ECHO :: ===============================================================
ECHO :: THE FOLLOWING CAN BE COPIED AND USED TO HELP RENAME THE ZIP
ECHO :: ZIP [ Epi Info 7 ] AND [ Launch Epi Info 7.exe ]
ECHO :: ===============================================================
ECHO %YYYY%.%MM%.%DD% Epi Info 7.2.4. (dual keys)(en-US,es-ES, fr-FR)

ENDLOCAL
GOTO :EOF

:HELP
ECHO.
ECHO :: ===============================================================
ECHO :: HELP
ECHO :: ===============================================================
ECHO.
ECHO Q   Quiet mode
ECHO K   Keep release keys
ECHO. 
ECHO To build a release version Epi Info 7 run the following: (requires Visual Studio and a private folder)
ECHO (.\epiinfo) epi-info-scorched-earth-build.bat [Q] [QK] [K] [/?]
ECHO .
GOTO :EOF

:EOF