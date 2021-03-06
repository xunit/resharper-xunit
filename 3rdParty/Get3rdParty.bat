@echo off

SET ProgFiles=%ProgramFiles(x86)%
if "%ProgFiles%"=="" SET ProgFiles=%ProgramFiles%

echo %ProgFiles%

:CopyResharper_v71

if not exist "%ProgFiles%\JetBrains\Resharper\v7.1\bin" goto CopyDotCover_v26

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
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Feature.Services.CSharp.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Feature.Services.VB.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Feature.Shared.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Psi.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Psi.CSharp.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Psi.VB.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.Resources.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.UnitTestExplorer.???" > nul
copy "%ProgFiles%\JetBrains\ReSharper\v7.1\Bin\JetBrains.ReSharper.UnitTestFramework.???" > nul
cd ..
echo Support for ReSharper 7.1 successfully copied.

:CopyDotCover_v26

if not exist "%ProgFiles%\JetBrains\dotCover\v2.6\bin" goto CopyDotCover_v27

mkdir dotCover_v2.6
cd dotCover_v2.6

copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.ComponentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.DocumentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.IDE.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.Metadata.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.ProjectModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.Shell.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.UI.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.Util.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.Platform.dotCover.VisualStudio.Core.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.Feature.Services.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.Features.Shared.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.Psi.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.Psi.CSharp.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.Psi.VB.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.Resources.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.TaskRunnerFramework.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.UnitTestExplorer.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.6\Bin\JetBrains.PsiFeatures.dotCover.UnitTestFramework.???" > nul
cd ..
echo Support for dotCover 2.6 successfully copied.

:CopyDotCover_v27

if not exist "%ProgFiles%\JetBrains\dotCover\v2.7\bin" goto End

mkdir dotCover_v2.7
cd dotCover_v2.7

copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.ComponentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.DocumentModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.IDE.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.Metadata.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.ProjectModel.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.Shell.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.UI.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.Util.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.Platform.dotCover.VisualStudio.Core.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.Feature.Services.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.Features.Shared.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.Psi.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.Psi.CSharp.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.Psi.VB.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.Resources.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.TaskRunnerFramework.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.UnitTestExplorer.???" > nul
copy "%ProgFiles%\JetBrains\dotCover\v2.7\Bin\JetBrains.PsiFeatures.dotCover.UnitTestFramework.???" > nul
cd ..
echo Support for dotCover 2.7 successfully copied.

goto End

:End
pause
