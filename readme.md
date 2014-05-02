## xUnit.net for ReSharper

This plugin for ReSharper adds support for xUnit.net tests. It supports the following:

* xUnit.net 1.x and *alpha quality* support for xUnit.net 2.0
* Discovery and execution of `[Fact]` and `[Theory]` based tests from source code, or compiled assemblies (e.g. F# projects can run tests, although tests aren't found in the source editor
* External annotations to provide hints to ReSharper that test methods are being implicitly used, and that assert methods check for null, etc.
* Live Templates to easily create asserts, test methods, etc.

Previously hosted on CodePlex as the [xunitcontrib](http://xunitcontrib.codeplex.com) project.

### Installing ###

The plugin can be installed from ReSharper's Extension Manager.

The stable version supports xUnit.net 1.x. The pre-release version (select the pre-release drop down in the Extension Manager) adds support for xUnit.net 2.0. 

### xUnit.net 2.0 support and caveats ###

The plugin currently adds initial, alpha-level support for xUnit.net 2.0.

See Pull Request #1 for progress on full support.

### License ###

This project is licensed as Apache 2.0. It was relicensed from Ms-PL on 02/05/2014. Previously licensed code is still available at commit b7aa82ad014b089da931a6ab5eb3017a43b5846d and earlier, and with the previous project page on [CodePlex](http://xunitcontrib.codeplex.com).