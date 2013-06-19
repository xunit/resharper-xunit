@echo off
setlocal enableextensions

set BIN=..\src\xunitcontrib.runner.resharper.provider\bin\Release
set ANNOTATIONS=..\ExternalAnnotations
set DOTCOVER=xunitcontrib-dotcover
set RESHARPER=xunitcontrib-resharper

mkdir %DOTCOVER%.2.0 2> NUL
copy /y %BIN%\*.2.0.* %DOTCOVER%.2.0\
copy /y %BIN%\xunit.dll %DOTCOVER%.2.0\
copy /y %BIN%\xunit.runner.utility.dll %DOTCOVER%.2.0\

mkdir %DOTCOVER%.2.1 2> NUL
copy /y %BIN%\*.2.1.* %DOTCOVER%.2.1\
copy /y %BIN%\xunit.dll %DOTCOVER%.2.1\
copy /y %BIN%\xunit.runner.utility.dll %DOTCOVER%.2.1\

mkdir %DOTCOVER%.2.2 2> NUL
copy /y %BIN%\*.2.2.* %DOTCOVER%.2.2\
copy /y %BIN%\xunit.dll %DOTCOVER%.2.2\
copy /y %BIN%\xunit.runner.utility.dll %DOTCOVER%.2.2\

mkdir %DOTCOVER%.2.5 2> NUL
copy /y %BIN%\*.2.5.* %DOTCOVER%.2.5\
copy /y %BIN%\xunit.dll %DOTCOVER%.2.5\
copy /y %BIN%\xunit.runner.utility.dll %DOTCOVER%.2.5\

mkdir %RESHARPER%.6.1 2> NUL
copy /y %BIN%\*.6.1.* %RESHARPER%.6.1\
copy /y %BIN%\xunit.dll %RESHARPER%.6.1\
copy /y %BIN%\xunit.runner.utility.dll %RESHARPER%.6.1\
copy /y %BIN%\*.dotSettings %RESHARPER%.6.1\
copy /y %ANNOTATIONS%\xunit.xml %RESHARPER%.6.1\

mkdir %RESHARPER%.7.1 2> NUL
copy /y %BIN%\*.7.1.* %RESHARPER%.7.1\
copy /y %BIN%\xunit.dll %RESHARPER%.7.1\
copy /y %BIN%\xunit.runner.utility.dll %RESHARPER%.7.1\
copy /y %BIN%\*.dotSettings %RESHARPER%.7.1\
copy /y %ANNOTATIONS%\xunit.xml %RESHARPER%.7.1\

copy /y ..\..\3rdParty\UnblockZoneIdentifier\*.*
