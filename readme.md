## xUnit.net for ReSharper

This plugin for ReSharper adds support for xUnit.net tests. It supports the following:

* xUnit.net 1.x and 2.0
* Discovery and execution of `[Fact]` and `[Theory]` based tests from source code, or compiled assemblies (e.g. F# projects can run tests, although tests aren't found in the source editor)
* External annotations to provide hints to ReSharper that test methods are being implicitly used, and that assert methods check for null, etc.
* Live Templates to easily create asserts, test methods, etc.
* Support for ReSharper 8.2 and 9.1. ReSharper 9.0 support is still available from the Extension Manager, but is no longer being maintained. Please upgrade to 9.1

Previously hosted on CodePlex as the [xunitcontrib](http://xunitcontrib.codeplex.com) project.

### Installing

The plugin can be installed from ReSharper's Extension Manager.

### xUnit.net 2.0 support and known issues

Both xUnit.net 1.x and 2.0 are fully supported when running tests, however, the 2.0 support has the following limitations in the editor:

* Test discovery is still based on 1.x. That means it will only find tests that are marked with `[Fact]` or other attributes that derive from `FactAttribute` (this includes `[Theory]`). Custom test discovery is not yet supported.
* The Live Templates are still based on 1.x. The biggest problem here is that `[Theory]` is in the wrong namespace.
* There is no code completion for `[MemberData]` has not yet been implemented. xUnit.net 1.x's `[PropertyData]` is still supported.

See [Pull Request #1](https://github.com/xunit/resharper-xunit/pull/1) for progress on full support.

### Building and contributing

See [contributing.md](contributing.md) for more details.

### License

This project is licensed as Apache 2.0. It was relicensed from Ms-PL on 02/05/2014. Previously licensed code is still available at commit b7aa82ad014b089da931a6ab5eb3017a43b5846d and earlier, and with the previous project page on [CodePlex](http://xunitcontrib.codeplex.com).
