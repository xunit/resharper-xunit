# Contributing and Building

Please feel free to send pull requests for new features. It is strongly recommended to follow the [GitHub Way](https://github.com/blog/1124-how-we-use-pull-requests-to-build-github) of using Pull Requests, which is to open a PR as early as possible. This allows for collaboration and discussion on a feature in progress, rather than a surprise delivery of a complete feature.

When building a ReSharper plugin, please consult the [devguide](https://www.jetbrains.com/resharper/devguide) - it has a lot of useful information, even if it's not a complete reference to the ReSharper API.

## Building

You should be able to fork or clone the repo, open the solution in Visual Studio, and build. The ReSharper SDKs are delivered as NuGet packages, and will be restored on first build.

There are several solutions that live in the repo, under the `\resharper` folder:

* `xunitcontrib-resharper.sln` - this is the main repository and contains projects for the test provider, and runner (see [Architecture](#architecture) below) as well as the test projects. There are two projects for each - one for ReSharper 8.2, and one for ReSharper 9.0.
* `xunitcontrib-resharper8.2.sln` - a solution for just ReSharper 8.2. It contains projects for the provider, the runner and the tests.
* `xunitcontrib-resharper9.0.sln` - as above - just the projects for ReSharper 9.0.
* `tests.manual.sln` - a solution that contains a load of "manual" tests. This can be used to manually test that ReSharper finds tests, marks them as in use, provides code completion and can actually run tests. This solution is used less as more tests are being automated.

### Building the installer nuget package

To create an extension nuget package, use the `xunitcontrib.nuspec` or `xunitcontrib-rs9.nuspec` files in the `\resharper\nuget` folder. Simply run `nuget pack xunitcontrib.nuspec` or `nuget pack xunitcontrib-rs9.nuspec`.

There are two different packages for 8.2 and 9.0 - `xunitcontrib.X.X.X.nupkg` and `CitizenMatt.Xunit.X.X.X.nupkg`. This is for two reasons:

* ReSharper 9 introduced a requirement that all extension packages have a "." in the name, so "xunitcontrib" is an invalid package name. (The first component before the dot is used as a company name)
* ReSharper uses "curated feeds" in the NuGet gallery to provide extensions that are compatible with a particular version of ReSharper. Unfortunately, curated feeds include all versions of a package that has, at some time, matched the criteria. This means, if a package no longer matches the criteria, it's still included in the curated feed, but is now no longer compatible. So, using the same package for ReSharper 8.2 and 9.0 would cause 8.2 users to have the 8.2 compatible version to be blocked. A new packages is used instead, and the curated feeds keep the packages hidden from the incompatible versions.

## Architecture

The solution is split into three projects, with copies of each for ReSharper 8.2 and 9.0:

* `\resharper\src\provider\provider-rsXX.csproj` - this is the unit test provider, which is loaded into the main ReSharper/Visual Studio process. It is responsible for finding tests and creating the tasks that are passed to the runner in order to execute them. It looks for tests both in source files, by examining the Abstract Syntax Tree of a file, and also in compiled assemblies. ReSharper is responsible for calling into the provider to keep the list of test elements up to date.
* `\resharper\src\runner\runner-rsXX.csproj` - the test runner, which is loaded into ReSharper's external test runner process. All tests are run in an external process (which might be 32 or 64 bit, .net 2 or .net 4). When starting a test run, the provider creates a set of tasks that are serialised and passed to the runner. The runner uses this information to load the appropriate version of xunit and run the required tests. Progress is passed back to the main ReSharper process, using ReSharper's API.
* `\resharper\test\src\tests\tests-rsXX.csproj` - automated tests. See [Testing](#testing) below.

## Testing

ReSharper supports automated testing, and the `tests` project makes use of that. ReSharper's test framework will create an in-memory ReSharper environment, that in turn creates a temporary solution, with a temporary project. Each test will then add a file from the `\resharper\test\data\...` folders to this project. This will either be a source file, for testing discovery in source files, or an assembly, for testing discovery in IL metadata or testing test execution (the assemblies are compiled and cached as part of the test, using the xunit assemblies in `\resharper\test\lib`).

When a test is run, the test framework will generate a `.tmp` file that contains the output of the test. This output may be specially formatted. For example, test discovery is a dump of test elements, giving the type, ID, and name of the discovered tests, test execution is the list of messages sent from the test runner to the ReSharper process. These `.tmp` files are compared against `.gold` files, and if they match, the `.tmp` file is deleted, and the test passes. If the `.tmp` file does not match, the test fails, and the `.tmp` file remains. ReSharper's test framework writes batch files to the `%TEMP%` directory that automate comparing the files and updating the gold files (this requires [kdiff](http://kdiff3.sourceforge.net/) to be installed). You'll probably need to clean up your `%TEMP%` directory from time to time...

> NOTE: ReSharper 9.0's tests are broken. The SDK's test environment is set up to run tests as if you're building the ReSharper product, and makes assumptions that aren't true for extension development. This can be worked around (but it's messy, which is why it's not checked in), and will be fixed for 9.1

More information on testing ReSharper plugins can be found in the [devguide](https://www.jetbrains.com/resharper/devguide/Plugins/Testing.html).

## Debugging

The extension can be installed locally in order to debug and test manually. The process is different for ReSharper 8.2 and 9.0.

### Debugging in 8.2

The extension cannot be debugged if it is already installed. The extension must be uninstalled.

Once uninstalled, the extension can be debugged, either by creating a nuget package and installing it locally (add a package source in ReSharper &rarr; Options &rarr; Extension Manager pointing to a local folder), or by using the `devenv /ReSharper.Plugin xunitcontrib.runner.resharper.provider.8.2.dll` command line (make sure you're in the debug directory, or pass in the full path to the `/ReSharper.Plugin` switch).

Note that when running the plugin with the `/ReSharper.Plugin` switch, any external annotations or settings files (for Live Templates) are not loaded. They are loaded when running as an installed extension.

### Debugging in 9.0

In order to debug with 9.0, the extension must be initially installed. Build the nuget package using the `xunitcontrib-rs9.nuspec`, and install it from a local packages source (ReSharper &rarr; Options &rarr; Extension Manager. You might also need to check the pre-release checkbox on this page). Once installed, the files can be updated manually, by copying them directly to the ReSharper install folder. This is done automatically for the provider assembly, but not the runner (I don't know why, actually, must fix that) or the xunit dependencies (dependencies aren't copied because that would also copy the ReSharper SDK assemblies).

The plugin can be installed to a separate "custom hive", meaning that the main hive can continue to have the normal released version of the plugin installed, but a custom hive can be used to debug and test a development version. See the devguide for more details on [debugging](https://www.jetbrains.com/resharper/devguide/Plugins/Debugging.html) and [installing locally](https://www.jetbrains.com/resharper/devguide/Extensions/Deployment/LocalInstallation.html).

