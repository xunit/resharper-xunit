@echo off

SET ProgFiles=%ProgramFiles(x86)%
if "%ProgFiles%"=="" SET ProgFiles=%ProgramFiles%

echo %ProgFiles%

if not exist "%ProgFiles%\JetBrains\Resharper\v6.1\bin" goto CopyResharper_v71

mkdir ReSharper_v6.1
cd ReSharper_v6.1

copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Annotations.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.ComponentModel.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.DocumentManager.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.DocumentModel.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.IDE.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.MetaData.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.ProjectModel.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.Shell.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.Resharper.UI.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.Platform.ReSharper.Util.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.Daemon.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.Features.Common.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.Feature.Services.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.Psi.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.Resources.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.UnitTestExplorer.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v6.1\Bin\JetBrains.ReSharper.UnitTestFramework.???" > nul
cd ..
echo Support for ReSharper 6.1 successfully copied.

:CopyResharper_v71

if not exist "%ProgFiles%\JetBrains\Resharper\v7.1\bin" goto CopyDotCover_v21

mkdir ReSharper_v7.1
cd ReSharper_v7.1

copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Annotations.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.ComponentModel.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.DocumentManager.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.DocumentModel.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.IDE.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.MetaData.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.ProjectModel.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.Shell.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.Resharper.UI.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.Platform.ReSharper.Util.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Daemon.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Daemon.Engine.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Features.Common.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Feature.Services.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Psi.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Resources.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.UnitTestExplorer.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.UnitTestFramework.???" > nul
cd ..
echo Support for ReSharper 7.1 successfully copied.

:CopyDotCover_v21

if not exist "%ProgFiles%\JetBrains\dotCover\v2.1\bin" goto CopyDotCover_v22

mkdir dotCover_v2.1
cd dotCover_v2.1

copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Annotations.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.ComponentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.DocumentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.IDE.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.MetaData.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.ProjectModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.Shell.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.UI.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.Platform.dotCover.Util.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.dotCover.Psi.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.dotCover.Resources.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.dotCover.TaskRunnerFramework.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.dotCover.UnitTestExplorer.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.1\Bin\JetBrains.dotCover.UnitTestFramework.???" > nul
cd ..
echo Support for dotCover 2.1 successfully copied.

:CopyDotCover_v22

if not exist "%ProgFiles%\JetBrains\dotCover\v2.2\bin" goto End

mkdir dotCover_v2.2
cd dotCover_v2.2

copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Annotations.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.ComponentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.DocumentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.IDE.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.MetaData.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.ProjectModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.Shell.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.UI.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.Platform.dotCover.Util.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.dotCover.Psi.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.dotCover.Resources.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.dotCover.TaskRunnerFramework.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.dotCover.UnitTestExplorer.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.2\Bin\JetBrains.dotCover.UnitTestFramework.???" > nul
cd ..
echo Support for dotCover 2.2 successfully copied.

goto End

:End
pause
