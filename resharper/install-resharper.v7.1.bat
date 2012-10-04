@echo off
setlocal enableextensions

set PROGFILES=%PROGRAMFILES(x86)%
if "%PROGFILES%"=="" set PROGFILES=%PROGRAMFILES%

set VERSION=7.1
set PRODUCT=resharper
set BASEDIR=JetBrains\%PRODUCT%\v%VERSION%

set INSTALL_SOURCEDIR=%~dp0\xunitcontrib.runner.%PRODUCT%.%VERSION%

set PER_MACHINE_PLUGINDIR=%PROGFILES%\%BASEDIR%\bin\plugins\xunitcontrib
set PER_USER_PLUGINDIR=%LOCALAPPDATA%\%BASEDIR%\plugins\xunitcontrib

if not exist "%PER_MACHINE_PLUGINDIR%" goto make_per_user_plugindir
rmdir /s /q "%PER_MACHINE_PLUGINDIR%"
if exist "%PER_MACHINE_PLUGINDIR%" (
    echo Unable to delete "%PER_MACHINE_PLUGINDIR%"
    echo Please run again as administrator
    goto :end
)

:make_per_user_plugindir
if exist "%PER_USER_PLUGINDIR%" goto do_copy
mkdir "%PER_USER_PLUGINDIR%"

:do_copy
echo Copying files...
copy /y "%INSTALL_SOURCEDIR%\*.dll" "%PER_USER_PLUGINDIR%"
copy /y "%INSTALL_SOURCEDIR%\*.pdb" "%PER_USER_PLUGINDIR%" 2> NUL

echo.

echo Unblocking downloaded files...
pushd "%PER_USER_PLUGINDIR%"
for /r %%i in (*) do "%~dp0\UnblockZoneIdentifier" %%i
popd

:end
pause
