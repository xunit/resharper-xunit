@echo off

SET ProgFiles=%ProgramFiles(x86)%
if "%ProgFiles%"=="" SET ProgFiles=%ProgramFiles%

SET BuildRoot=resharper60\xunitcontrib.runner.resharper.provider.ide.6.0\bin\Debug\
SET ReSharperRoot=%ProgFiles%\JetBrains\ReSharper\v6.0\bin\
SET PluginsRoot=%ReSharperRoot%plugins\xunitcontrib\
SET ExternalAnnotationsRoot=%ReSharperRoot%ExternalAnnotations\

rmdir /s /q "%PluginsRoot%"

REM Installed to %ReSharperRoot% due to a breaking change in rs60 EAP
del "%ReSharperRoot%xunit.runner.utility.dll"
del "%ReSharperRoot%xunitcontrib.runner.resharper.runner.6.0.dll"
del "%ReSharperRoot%xunitcontrib.runner.resharper.runner.6.0.pdb"

del "%ExternalAnnotationsRoot%xunit.xml"

pause
