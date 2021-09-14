@echo off
set BUILD_DIR=C:\Work\SVN\altgame\build\
set CHECKUTILITY=%BUILD_DIR%bin\CheckConsole.exe
set BUILDUTILITY=%BUILD_DIR%BuildUtility\bin\Debug\BuildUtility.exe
rem Check if console is started from top and then pause at end - assuming new console window was created
set PAUSER=pause
%CHECKUTILITY%
set LINE=%ERRORLEVEL%
if %LINE% GTR 2 (
	set PAUSER=
)
echo --- Prepare script ---
set REBUILD=
set NO_SVN=0
set COPY=0
set TEST=0
set SKIP=0
:parse_params
set P1=%1
if "%P1%" == "" goto :parse_done
set PARSE=false
if "%P1%" == "-rebuild" (
    echo !
    echo ! Doing REBUILD with -nosvn
    echo !
	set REBUILD=-rebuild
	set NO_SVN=1
	set PARSE=true
)
if "%P1%" == "-nosvn" (
    echo !
    echo ! Skip SVN
    echo !
	set NO_SVN=1
	set PARSE=true
)
if "%P1%" == "-copy" (
    echo !
    echo ! Just COPY output
    echo !
    set COPY=1
    set REBUILD=
    set NO_SVN=1
    set PARSE=true
)
if "%P1%" == "-test" (
    echo.
    echo --- TEST mode ON ---
    set TEST=1
    set PARSE=true
)
if "%P1%" == "-skip" (
    echo.
    echo --- Skip UNITY build for testing ---
    set SKIP=1
    set PARSE=true
)
if %PARSE% == false (
    echo Unknown argument: %P1%
    goto :parse_help
)
shift
goto :parse_params

:parse_help
echo.
echo Usage: %0 [options]
echo Options:
echo -rebuild   rebuild project with current settings (implies -nosvn)
echo -nosvn     do not get latest form SVN
echo -copy      just copy OUTPUT
echo -test      test script before UNITY build
echo -skip      test script before but skip UNITY build
goto :exit_build

:parse_done
if %NO_SVN% == 1 (
    goto :svn_done
)
echo.
echo SVN update
echo -----------------------------------
svn update
echo -----------------------------------

:svn_done
echo.
echo Check build type file
if NOT exist m_BuildType (
    type nul >m_BuildType
    echo *
    echo * Build FAILED, m_BuildType file not found
    echo *
    echo * Empty m_BuildType file is created for your convenience to fix it
    echo *
    goto :buildtype_help
)
set BUILD=
set /p BUILD=<m_BuildType
if "%BUILD%" == "" (
    echo *
    echo * Build FAILED, m_BuildType file not found or is empty
    echo *
    goto :buildtype_help
)
if "%BUILD%" == "Android" goto :valid_build
if "%BUILD%" == "WebGL" goto :valid_build
if "%BUILD%" == "StandaloneWindows64" goto :valid_build
echo *
echo * Build FAILED, invalid m_BuildType '%BUILD%'
:buildtype_help
echo * m_BuildType must be one of UNITY BuildTarget enum values in single line
echo *
echo * Supported build types are:
echo *	Android
echo *	WebGL
echo *	StandaloneWindows64
echo *
goto :exit_build

:valid_build
set BUILDOUTPUT=Build%BUILD%
set BUILDLOG=build_%BUILD%_unity.log
set ROBOCOPYLOG=build_%BUILD%_dropbox.log
set DROPBOX=
if exist m_DropboxFolder (
	set /p DROPBOX=<m_DropboxFolder
)
if exist "%DROPBOX%" (
    echo.
    echo Dropbox folder is %DROPBOX%
) else (
    echo.
    echo Dropbox is NOT used
)
if %COPY% == 1 (
    echo.
    echo +
    echo + Copy %BUILD% to %BUILDOUTPUT%
    echo +
	goto :copy_dropbox
)
set BUILDOPTIONS1=-build %BUILD% %REBUILD%
echo BUILDOPTIONS1 %BUILDOPTIONS1%
echo.
echo --- Update project settings and environment ---
call %BUILDUTILITY% -preBuild %BUILDOPTIONS1%

set EDITOR=
set /p EDITOR=<m_EditorFile
if "%EDITOR%" == "" (
    echo *
    echo * Build FAILED, m_EditorFile file not found
    echo *
	goto :exit_build
)
set BUILDNUMBER=
set /p BUILDNUMBER=<m_BuildNumber
if "%BUILDNUMBER%" == "" (
    echo *
    echo * Build FAILED, m_BuildNumber file not found
    echo * %BUILDUTILITY% should create it
    echo *
	goto :exit_build
)

if %TEST% == 1 (
    echo.
    echo --- TEST mode EXIT ---
	goto :exit_build
)

echo.
echo --- Execute script operations ---

set PROJECTPATH=.

if exist "%BUILDOUTPUT%" (
    echo.
    echo Delete %BUILDOUTPUT%
	rmdir /Q /S "%BUILDOUTPUT%"
)
if exist "%BUILDLOG%" (
    echo.
    echo Delete %BUILDLOG%
	del /Q "%BUILDLOG%"
)
if exist "%ROBOCOPYLOG%" (
    echo.
    echo Delete %ROBOCOPYLOG%
	del /Q "%ROBOCOPYLOG%"
)
if exist m_LocationPathName (
	del /Q m_LocationPathName
)
if exist m_ProjectFolderPath (
	del /Q m_ProjectFolderPath
)
if exist m_BuildStatistics (
	del /Q m_BuildStatistics
)
set METHOD=Editor.Prg.BatchBuild.BatchBuildPlayer.build
set UNITY_OPTIONS=-quit -batchmode -executeMethod %METHOD% -projectPath %PROJECTPATH% -buildTarget %BUILD% -logFile "%BUILDLOG%"
set CUSTOM_OPTIONS=-myBuildNumber %BUILDNUMBER% %REBUILD% -myLocalFolder local_%USERNAME% -myLogPrefix BUILD_LOG
echo.
echo UNITY  OPTIONS %UNITY_OPTIONS%
echo CUSTOM OPTIONS %CUSTOM_OPTIONS%
echo UNITY %EDITOR%
echo.
time /T
echo Build %BUILD% output %BUILDOUTPUT%
if %SKIP% == 1 (
    echo.
    echo --- SKIPPING UNITY BUILD ---
	mkdir "%BUILDOUTPUT%"
    echo dymmy_content >"%BUILDLOG%"
	copy "%BUILDLOG%" "%BUILDOUTPUT%"
    echo x>m_LocationPathName
    echo x>m_ProjectFolderPath
    echo x>m_BuildStatistics
) else (
	"%EDITOR%" %UNITY_OPTIONS% %CUSTOM_OPTIONS%
)
time /T
if exist m_BatchBuildPlayerStatus (
    echo !
    echo ! Build did not complete succesfully!
    echo !
	type m_BuildStatistics
)
if NOT exist "%BUILDOUTPUT%" (
    echo *
    echo * Build failed, no OUTPUT %BUILDOUTPUT% created
    echo *
	goto :exit_build
)
echo.
echo +
echo + UNITY Build SUCCESS %BUILD%
echo +
if exist m_BuildStatistics (
    echo.
	type m_BuildStatistics
) else (
    echo.
    echo m_BuildStatistics NOT FOUND
)
echo.
set /p m_ProjectFolderPath=<m_ProjectFolderPath
echo m_ProjectFolderPath=%m_ProjectFolderPath%
set /p m_LocationPathName=<m_LocationPathName
echo m_LocationPathName=%m_LocationPathName%
echo.
echo --- Run post build processing ---
set BUILDOPTIONS2=-projectFolderPath %m_ProjectFolderPath% -locationPathName %m_LocationPathName%
echo BUILDOPTIONS2 %BUILDOPTIONS2%
call %BUILDUTILITY% -postBuild %BUILDOPTIONS1% %BUILDOPTIONS2%

:copy_dropbox
if exist "%DROPBOX%" (
	if exist "%BUILDOUTPUT%" (
	    echo.
	    echo Copy files to Dropbox
		robocopy "%BUILDOUTPUT%" "%DROPBOX%" /S /E /V /NP /LOG:%ROBOCOPYLOG%
	)
) else (
    echo.
    echo Dropbox was SKIPPED
)
echo.
echo ALL operations done
echo.
echo Check SVN status and commit changes
echo -----------------------------------
svn status
echo -----------------------------------
goto :exit_build

:exit_build
%PAUSER%
goto :eof
