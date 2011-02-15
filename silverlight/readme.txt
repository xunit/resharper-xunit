What's working?
===============

1. The bare essentials. Should be everything to get you going. Facts, asserts, theories.
   Each test runs in a new instance of the test class. IUseFixture<> and IDisposable are
   fully supported
2. Support for Silverlight 3 and 4, using the April 2010 toolkit (Silverlight 3 support
   comes from an semi-official build of the April toolkit - see Jeff Wilcox's blog post
   http://www.jeff.wilcox.name/2010/05/sl3-utf-bits/)
3. The Silverlight toolkit's Exclusive attribute is supported. This allows you to run only
   some tests and not all of them. You need to put the Exclusive attribute on the class,
   and then on the test methods you want to run
4. Owner and Description metadata are supported via traits with the same name. Only Description
   is surfaced in the UI. Owner gets output to the Visual Studio trace log (which I'm not sure
   is working in the April 2010 framework). Category is also populated from traits, but isn't
   used by the framework

What's not working?
===================

1. OleDb based theories, due to lack of OleDb support in Silverlight
2. Capturing output (i.e. stdout, stderr, debug tracing)
3. If you're in UTC, the clock tests don't work. This is a bug in xunit
4. The version independent runner. There is enough of a version of XmlNode to get the tests
   to pass, but nothing further (yet?)
5. Tests are not run in random order, like in the desktop framework. This is because the
   Silverlight unit testing framework handles the actual running of tests
6. The xunitcontrib resharper runner thinks it can run these silverlight tests. It can't.
7. Windows Phone 7 support (yet)
8. Support for the Silverlight unit testing framework's base classes

What's not been tested?
=======================

1. The Asynchronous attribute + testing controls or other UI. Should work, though
2. Integration with statlight + agunit
3. Bug + Tag attributes

What do I need to know?
=======================

1. Install the April 2010 toolkit. This gives you a project template for Silverlight Unit Tests
2. Create a unit test project. Add a reference to xunit-silerlight4, xunit.extensions-silverlight4
   (for theory support) and xunitcontrib.runner.silverlight.toolkit-silverlight4 (the unit test
   provider for the unit testing framework)
3. Register the test runner. In App.xaml.cs, before the call to UnitTestSystem.CreateTestPage(),
   call XunitContrib.Runner.Silverlight.Toolkit.UnitTestProvider.Register()

* To use Silverlight 3, replace the reference to Microsoft.Silverlight.Testing with the one
  in 3rdParty\MicrosoftSilverlightTesting-05-2010-SL3 (copied from Jeff Wilcox's blog
  http://www.jeff.wilcox.name/2010/05/sl3-utf-bits/) and replace the references to the xunit
  and xunitcontrib assemblies with the -silverlight3 versions. Other than that, it's exactly
  the same

* Silverlight only lets you use Reflection for late binding. You can't use it to access members
  that you wouldn't have access to at compile time. This means you might need to use the 
  InternalsVisibleTo attribute to allow xunit-silverlight3 or xunit-silverlight4 to get access
  to your internal members (private members are just off limits).
