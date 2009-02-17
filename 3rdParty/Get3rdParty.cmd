@echo off

if exist "%ProgramFiles%\JetBrains\Resharper\v4.1\bin"      goto CopyResharper_x86
if exist "%ProgramFiles(x86)%\JetBrains\Resharper\v4.1\bin" goto CopyResharper_x64
if exist "%ProgramFiles%\JetBrains\Resharper\v4.5\bin"      goto CopyResharper_v45_x86
if exist "%ProgramFiles%\JetBrains\Resharper\v4.5\bin"      goto CopyResharper_v45_x64
echo *** ERROR: Could not locate ReSharper 4.1 ***
exit /B 1

:CopyResharper_x86
mkdir ReSharper_v4.1
cd ReSharper_v4.1
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.DocumentManager.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.IDE.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.MetaData.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.ProjectModel.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.Shell.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.Resharper.UI.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.Util.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.CodeInsight.Services.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.CodeView.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.Psi.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.TestFramework.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.UnitTestExplorer.???" > nul
echo Support for ReSharper 4.1 successfully copied.
goto End

:CopyResharper_v45_x86
mkdir ReSharper_v4.5
cd ReSharper_v4.5
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.Platform.ReSharper.DocumentManager.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.Platform.ReSharper.IDE.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.Platform.ReSharper.MetaData.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.Platform.ReSharper.ProjectModel.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.Platform.ReSharper.Shell.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.Platform.Resharper.UI.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.Platform.ReSharper.Util.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.ReSharper.CodeInsight.Services.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.ReSharper.CodeView.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.ReSharper.Psi.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.ReSharper.TestFramework.???" > nul
copy "%ProgramFiles%\JetBrains\ReSharper\v4.5\Bin\JetBrains.ReSharper.UnitTestExplorer.???" > nul
echo Support for ReSharper 4.5 successfully copied.
goto End

:CopyResharper_x64
mkdir ReSharper_v4.1
cd ReSharper_v4.1
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.DocumentManager.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.IDE.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.MetaData.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.ProjectModel.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.Shell.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.Resharper.UI.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.Util.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.CodeInsight.Services.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.CodeView.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.Psi.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.TestFramework.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.UnitTestExplorer.???" > nul
echo Support for ReSharper 4.1 successfully copied. [Platform=x64]
goto End

:CopyResharper_x64
mkdir ReSharper_v4.5
cd ReSharper_v4.5
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.DocumentManager.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.IDE.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.MetaData.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.ProjectModel.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.Shell.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.Resharper.UI.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.Platform.ReSharper.Util.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.CodeInsight.Services.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.CodeView.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.Psi.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.TestFramework.???" > nul
copy "%ProgramFiles(x86)%\JetBrains\ReSharper\v4.1\Bin\JetBrains.ReSharper.UnitTestExplorer.???" > nul
echo Support for ReSharper 4.5 successfully copied. [Platform=x64]
goto End

:End