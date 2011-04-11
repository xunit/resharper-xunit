@echo off

SET ProgFiles=%ProgramFiles(x86)%
if "%ProgFiles%"=="" SET ProgFiles=%ProgramFiles%

SET BuildRoot=resharper60\xunitcontrib.runner.resharper.provider.ide.6.0\bin\Debug\
SET ReSharperRoot=%ProgFiles%\JetBrains\ReSharper\v6.0\bin\
SET PluginsRoot=%ReSharperRoot%plugins\xunitcontrib\
SET ExternalAnnotationsRoot=%ReSharperRoot%ExternalAnnotations\

if not exist "%PluginsRoot%" mkdir "%PluginsRoot%"

copy %BuildRoot%xunit.dll "%PluginsRoot%"
copy %BuildRoot%xunitcontrib.runner.resharper.provider.metadata.6.0.dll "%PluginsRoot%"
copy %BuildRoot%xunitcontrib.runner.resharper.provider.metadata.6.0.pdb "%PluginsRoot%"
copy %BuildRoot%xunitcontrib.runner.resharper.provider.ide.6.0.dll "%PluginsRoot%"
copy %BuildRoot%xunitcontrib.runner.resharper.provider.ide.6.0.pdb "%PluginsRoot%"

REM Installed to %ReSharperRoot% due to a breaking change in rs60 EAP
copy %BuildRoot%xunit.runner.utility.dll "%ReSharperRoot%"
copy %BuildRoot%xunitcontrib.runner.resharper.runner.6.0.dll "%ReSharperRoot%"
copy %BuildRoot%xunitcontrib.runner.resharper.runner.6.0.pdb "%ReSharperRoot%"

copy ExternalAnnotations\xunit.xml "%ExternalAnnotationsRoot%"

pause
