What's working
==============

What's working?
1. The bare essentials. Should be everything to get your going. Facts, asserts, theories.
2. Support for Silverlight 3 and 4, using the April 2010 toolkit (Silverlight 3 support
   comes from an semi-official build of the April toolkit - see Jeff Wilcox's blog post
   http://www.jeff.wilcox.name/2010/05/sl3-utf-bits/)

What's not working?
1. OleDb based theories, due to lack of OleDb support in Silverlight
2. Test timeouts (yet)
3. If you're in UTC, the clock tests don't work. This is a bug in xunit
4. The version independent runner, due to lack of XmlNode in Silverlight (yet, hopefully)
5. Er, possibly more stuff that I haven't found yet
6. The xunitcontrib resharper runner thinks it can run these silverlight tests. It can't.

What's not been tested?
1. Windows Phone 7 support
2. Integration with statlight + agunit

What do I need to know?
1. Install the April 2010 toolkit. This gives you a project template for Silverlight Unit Tests
2. Create a unit test project. Add a reference to xunit-silerlight4, xunit.extensions-silverlight4
   (for theory support) and xunitcontrib.runner.silverlight.toolkit-silverlight4 (the unit test
   provider for the unit testing framework)
3. Register the test runner. In App.xaml.cs, before the call to UnitTestSystem.CreateTestPage(),
   call XunitContrib.Runner.Silverlight.Toolkit.UnitTestProvider.Register()

* To use Silverlight 3, replace the reference to Microsoft.Silverlight.Testing with the one
  from Jeff Wilcox's blog (http://www.jeff.wilcox.name/2010/05/sl3-utf-bits/) and replace the
  xunit/xunitcontrib references with the -silverlight3 versions. Other than that, it's just the
  same

* Silverlight only lets you use Reflection for late binding. You can't use it to access members
  that you wouldn't have access to at compile time. This means you might need to use the 
  InternalsVisibleTo attribute to allow xunit-silverlight3 or xunit-silverlight4 to get access
  to your internal members (private members are just off limits).

How to build
============

Here are the steps required to build the port of xunit + the unit testing framework plugin

NOTE: This is a work in progress! There are currently many changes required in the xunit source.
I'm hoping this will reduce as time goes on. Please keep an eye on this file when getting the
latest version of the source

NOTE2: There are currently 18 failing tests for test.xunit + 8 for test.xunit.extensions. Remember 
that "work in progress" bit? Yeah.

NOTE3: When I comment stuff out of the xunit source, I tend to use #if !SILVERLIGHT. Just saying.

1. Clone the xunit repository and put the whole thing in a folder called "external" in 
   <repository_root>\silverlight. You should end up with a <repository_root>\silverlight\external\xunit\<xunit_repository>

2. These are the changes we Have To Do:
2a. Comment out the protected constructor in ParameterCountMismatchException - SerializationInfo is
    an internal class in Silverlight
2b. Comment out the contents of ExceptionUtility.RethrowWithNoStackTraceLoss. Keep the method.
    Then (and this is IMPORTANT) add a "throw;" everywhere that RethrowWithNo... was being called
    * This is because xunit is using private reflection to allow them to rethrow an exception without
      replacing the stack trace. Nice hack, but private reflection doesn't work in Silverlight.
      This means that xunit will now start throwing TargetInvocationExceptions instead of the actual
      exception. Not ideal, but there's no way around it.
2c. Change the assert in ReflectorTests+Invoke.ThrowsException to look at the inner exception. This
    is because of the above change - the test is expecting the real exception, but we're stuck with
    TargetInvocationException. 

3. These are the changes we have to do Right Now. I want to get rid of this, as best I can.
   Comment out the ENTIRE CONTENTS of the following files:
   Main\test.xunit\Sdk\Commands\TestCommands\FactCommandTests.cs
   Main\test.xunit\Sdk\Commands\TestCommands\TestCommandFactoryTests.cs
   Main\test.xunit\Sdk\Commands\TestCommands\TestCommandTests.cs
   Main\test.xunit\Sdk\Commands\TestCommands\TimeoutCommandTests.cs
   Main\test.xunit\Sdk\ExecutorCallbackTests.cs
   Main\test.xunit\Sdk\Results\AssemblyResultTests.cs
   Main\test.xunit\Stubs\StubTestCommand.cs
   Main\test.utility\StubTestRunner.cs
   Main\test.utility\StubTransformer.cs
   Main\test.xunit\SerializationTests.cs
   Main\xunit.runner.utility\Transformers\XslStreamTransformer.cs
3a. Do some surgical commenting out for the following files:
    Main\test.xunit\Sdk\Results\MethodResultTests.cs
      Comment out FindTrait()
      Comment out ToXmlWithTraits()

And um, that's it. Easy, right?

Now, just build, and run the tests. 

YOU SHOULD HAVE ONE FAILING TEST! 

This is intentional. If you don't have any failing tests, something's gone wrong, most likely exceptions
are getting unintentionally swallowed. There are some tests to try and detect this, but if we can't throw
exceptions, it's going to be a bit hard to fail tests.
