using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Application;
using JetBrains.CommonControls;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.UI;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;
using JetBrains.Util;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [UnitTestProvider]
    class XunitTestProvider : IUnitTestProvider
    {
        private static readonly XunitBrowserPresenter Presenter = new XunitBrowserPresenter();
        private static readonly AssemblyLoader AssemblyLoader = new AssemblyLoader();

        static XunitTestProvider()
        {
            // ReSharper automatically adds all test providers to the list of assemblies it uses
            // to handle the AppDomain.Resolve event

            // The test runner process talks to devenv/resharper via remoting, and so devenv needs
            // to be able to resolve the remote assembly to be able to recreate the serialised types.
            // (Aside: the assembly is already loaded - why does it need to resolve it again?)
            // ReSharper automatically adds all unit test provider assemblies to the list of assemblies
            // it uses to handle the AppDomain.Resolve event. Since we've got a second assembly to
            // live in the remote process, we need to add this to the list.
            AssemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }

        public string ID
        {
            get { return XunitTaskRunner.RunnerId; }
        }

        public string Name
        {
            get { return "xUnit.net"; }
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public int CompareUnitTestElements(UnitTestElement x, UnitTestElement y)
        {
            if (Equals(x, y))
                return 0;

            int compare = StringComparer.CurrentCultureIgnoreCase.Compare(x.GetTypeClrName(), y.GetTypeClrName());
            if (compare != 0)
                return compare;

            if (x is XunitTestElementMethod && y is XunitTestElementClass)
                return -1;

            if (x is XunitTestElementClass && y is XunitTestElementMethod)
                return 1;

            if (x is XunitTestElementClass && y is XunitTestElementClass)
                return 0;

            var xe = (XunitTestElementMethod)x;
            var ye = (XunitTestElementMethod)y;
            return xe.Order.CompareTo(ye.Order);
        }

        public UnitTestElement Deserialize(ISolution solution, string elementString)
        {
            return null;
        }

        public void ExploreAssembly(IMetadataAssembly assembly,
                                    IProject project,
                                    UnitTestElementConsumer consumer)
        {
            // This method gives us the Reflection-style metadata of a physical assembly,
            // and is called at start up (if the assembly exists) and whenever the assembly
            // is recompiled. It allows us to retrieve the tests that will actually get
            // executed, as opposed to ExploreFile, which gets the tests that exist in a
            // source file.
            // It would be nice to check to see that the assembly references xunit before iterating
            // through all the types in the assembly - a little optimisation. Unfortunately,
            // when an assembly is compiled, only assemblies that have types that are directly
            // referenced are embedded as references. In other words, if I use something from
            // xunit.extensions, but not from xunit (say I only use a DerivedFactAttribute),
            // then only xunit.extensions is listed as a referenced assembly. xunit will still
            // get loaded at runtime, because it's a referenced assembly of xunit.extensions.
            // It's also needed at compile time, but it's not a direct reference.
            // So I'd need to recurse into the referenced assemblies references, and I don't
            // quite know how to do that, and it's suddenly making our little optimisation
            // rather complicated. So (at least for now) we'll leave well enough alone and
            // just explore all the types
            //if(!ReferencesXUnit(assembly))
            //    return;

            // Create an appdomain
            // Create a class inside the appdomain
            // In the app domain:
            //   1. Load the assembly under test
            //   2. iterate over assembly.GetExportedTypes()
            //   3. find TestClassCommandFactory + call static Make method + store returned object (ITestClassCommand)
            //   4. foreach (object in object.EnumerateTestMethods).EnumerateTestCommands)
            //   5. foreach test command object, get DisplayName, MethodName and TypeName

            //using(ExecutorWrapper wrapper = new ExecutorWrapper(assembly.Location, null, false))
            //{
            //    XmlNode tests = wrapper.EnumerateTests();

            //    int i = 0;
            //    i++;
            //}

            foreach (IMetadataTypeInfo type in GetExportedTypes(assembly.GetTypes()))
            {
                ITypeInfo typeInfo = TypeWrapper.Wrap(type);
                if (!TypeUtility.IsTestClass(typeInfo) || TypeUtility.HasRunWith(typeInfo))
                    continue;

                ITestClassCommand command = TestClassCommandFactory.Make(typeInfo);
                if (command == null)
                    continue;

                ExploreTestClass(assembly, type, command.EnumerateTestMethods(), project, consumer);
            }
        }

        // We want to mimic xunit as closely as possible. They iterate over Assembly.GetExportedTypes to 
        // get candidate test methods. ReSharper seems to have a parallel API to walk through an assembly's
        // types, but their GetExportedTypes always returns an empty list for me. So, we'll just create our
        // own, based on GetTypes and the knowledge (as per msdn) that Assembly.GetExportTypes is looking for
        // "The only types visible outside an assembly are public types and public types nested within other public types."
        // TODO: It might be nice to randomise this list
        // However, this returns items in alphabetical ordering. Assembly.GetExportedTypes returns back in
        // the order in which classes are compiled (so the order in which their files appear in the msbuild file!)
        // with dependencies appearing first. 
        private static IEnumerable<IMetadataTypeInfo> GetExportedTypes(IEnumerable<IMetadataTypeInfo> types)
        {
            foreach (IMetadataTypeInfo type in types ?? new IMetadataTypeInfo[0])
            {
                if (!IsPublic(type)) continue;

                foreach (IMetadataTypeInfo nestedType in GetExportedTypes(type.GetNestedTypes()))
                {
                    if (IsPublic(nestedType))
                        yield return nestedType;
                }

                yield return type;
            }
        }

        private static bool IsPublic(IMetadataTypeInfo type)
        {
            // Hmmm. This seems a little odd. Resharper reports public nested types with IsNestedPublic,
            // while IsPublic is false
            return (type.IsNested && type.IsNestedPublic) || type.IsPublic;
        }

        public ProviderCustomOptionsControl GetCustomOptionsControl(ISolution solution)
        {
            return null;
        }

        public void ExploreExternal(UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore anything that's not a part of the solution + projects world
        }

        public void ExploreFile(IFile psiFile,
                                UnitTestElementLocationConsumer consumer,
                                CheckForInterrupt interrupted)
        {
            if (psiFile == null)
                throw new ArgumentNullException("psiFile");

            psiFile.ProcessDescendants(new XunitFileExplorer(this, consumer, psiFile, interrupted));
        }

        public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore the solution, without going into the projects
        }

        void ExploreTestClass(IMetadataAssembly assembly,
                              IMetadataTypeInfo type,
                              IEnumerable<IMethodInfo> methods,
                              IProjectModelElement project,
                              UnitTestElementConsumer consumer)
        {
            var @class = new XunitTestElementClass(this, project, type.FullyQualifiedName, assembly.Location);
            consumer(@class);

            int order = 0;
            foreach (IMethodInfo method in methods.Where(MethodUtility.IsTest))
            {
                ExploreTestMethod(project, @class, consumer, method, order++);
            }
        }

        void ExploreTestMethod(IProjectModelElement project,
                               XunitTestElementClass @class,
                               UnitTestElementConsumer consumer,
                               IMethodInfo method,
                               int order)
        {
            var test = new XunitTestElementMethod(this, @class, project, method.TypeName, method.Name, order);
            test.SetExplicit(MethodUtility.GetSkipReason(method));
            consumer(test);
        }

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        // This method gets called to generate the tasks that the remote runner will execute
        // When we run all the tests in a class (by e.g. clicking the menu in the margin marker)
        // this method is called with a class element and the list of explicit elements contains
        // one item - the class. We should add all tasks required to prepare to run this class
        // (e.g. loading the assembly and loading the class via reflection - but NOT running the
        // test methods)
        // It is then subsequently called with all method elements, and with the same list (that
        // only has the class as an explicit element). We should return a new sequence of tasks
        // that would allow running the test methods. This will probably be a superset of the class
        // sequence of tasks (e.g. load the assembly, load the class via reflection and a task
        // to run the given method)
        // Because the tasks implement IEquatable, they can be compared to collapse the multiple
        // lists of tasks into a tree (or set of trees) with only one instance of each unique
        // task in order, so in the example, we load the assmbly once, then we load the class once,
        // and then we have multiple tasks for each running test method. If there are multiple classes
        // run, then multiple class lists will be created and collapsed under a single assembly node.
        // If multiple assemblies are run, then there will be multiple top level assembly nodes, each
        // with one or more uniqe class nodes, each with one or more unique test method nodes
        // If you explicitly run a test method from its menu, then it will appear in the list of
        // explicit elements.
        // ReSharper provides an AssemblyLoadTask that can be used to load the assembly via reflection.
        // In the remote runner process, this task is serviced by a runner that will create an app domain
        // shadow copy the files (which we can affect via ProfferConfiguration) and then cause the rest
        // of the nodes (e.g. the classes + methods) to get executed (by serializing the nodes, containing
        // the remote tasks from these lists, over into the new app domain). This is the default way the
        // nunit and mstest providers work.
        public IList<UnitTestTask> GetTaskSequence(UnitTestElement element, IList<UnitTestElement> explicitElements)
        {
            var testMethod = element as XunitTestElementMethod;
            if (testMethod != null)
            {
                XunitTestElementClass testClass = testMethod.Class;
                var result = new List<UnitTestTask>
                                 {
                                     new UnitTestTask(null, CreateAssemblyTask(testClass.AssemblyLocation)),
                                     new UnitTestTask(testClass, CreateClassTask(testClass, explicitElements)),
                                     new UnitTestTask(testMethod, CreateMethodTask(testMethod, explicitElements))
                                 };

                return result;
            }

            // We don't have to do anything explicit for a test class, because when a class is run
            // we get called for each method, and each method already adds everything we need (loading
            // the assembly and the class)
            if (element is XunitTestElementClass)
                return EmptyArray<UnitTestTask>.Instance;

            throw new ArgumentException(string.Format("element is not xUnit: '{0}'", element));
        }

        private static RemoteTask CreateAssemblyTask(string assemblyLocation)
        {
            return new XunitTestAssemblyTask(assemblyLocation);
        }

        private static RemoteTask CreateClassTask(XunitTestElementClass testClass, ICollection<UnitTestElement> explicitElements)
        {
            return new XunitTestClassTask(testClass.AssemblyLocation, testClass.GetTypeClrName(), explicitElements.Contains(testClass));
        }

        private static RemoteTask CreateMethodTask(XunitTestElementMethod testMethod, ICollection<UnitTestElement> explicitElements)
        {
            return new XunitTestMethodTask(testMethod.Class.AssemblyLocation, testMethod.Class.GetTypeClrName(), testMethod.MethodName, explicitElements.Contains(testMethod));
        }

        public bool IsUnitTestElement(IDeclaredElement element)
        {
            return IsUnitTest(element) || IsUnitTestContainer(element);
        }

        // This method and IsUnitTestContainer appear to get called during e.g. code completion, so that
        // the appropriate icon can be displayed in the drop down list (class, method, property, test method, test "container")
        public bool IsUnitTest(IDeclaredElement element)
        {
            var testMethod = element as IMethod;
            bool isUnitTest = testMethod != null && MethodUtility.IsTest(MethodWrapper.Wrap(testMethod));
            System.Diagnostics.Debugger.Log(0, "hello", string.Format("IsUnitTest: element {0} - returning {1}{2}", element, isUnitTest, Environment.NewLine));
            return isUnitTest;
        }

        // This method and IsUnitTest appear to get called during e.g. code completion, so that
        // the appropriate icon can be displayed in the drop down list (class, method, property, test method, test "container")
        public bool IsUnitTestContainer(IDeclaredElement element)
        {
            var testClass = element as IClass;
            bool isUnitTestContainer = testClass != null && TypeUtility.IsTestClass(TypeWrapper.Wrap(testClass));
            System.Diagnostics.Debugger.Log(0, "hello", string.Format("IsUnitTestContainer: element {0} - returning {1}{2}", element, isUnitTestContainer, Environment.NewLine));
            return isUnitTestContainer;
        }

#if false
        public bool IsUnitTestStuff(IDeclaredElement element)
        {
            // There is an outstanding bug report on Jira for this - RSRP-101582
            // If we have a non-test project with a nested class, which has a method that is in use,
            // and Solution Wide Analysis is enabled, the nested class is displayed as in use, as
            // is the parent class. On top of that, the parent class says it can be made sealed.
            // If we have the same scenario for test classes, the parent class is marked as
            // unused. I think it should behave the same way - I don't really like having to walk
            // all nested classes within a class to see if it should be in use or not. Extrapolating
            // out, what happens if they add an analysis to say that the file is no longer in use?
            bool isUnitTestElement = false;

            var elementAsClass = element as IClass;
            if(elementAsClass != null)
            {
                isUnitTestElement = elementAsClass.NestedTypes.Aggregate(isUnitTestElement, (current, nestedType) => current | IsUnitTestElement(nestedType));
            }

            return IsUnitTestElement(element) | isUnitTestElement;
        }
#endif

        public void Present(UnitTestElement element,
                            IPresentableItem presentableItem,
                            TreeModelNode node,
                            PresentationState state)
        {
            Presenter.UpdateItem(element, node, presentableItem, state);
        }

        // Allows us to affect the configuration of a test run, specifying where to start running
        // and whether to shadow copy or not. Interestingly, it looks like we get called even
        // if we don't have any of our kind of tests in this run (so I could affect an nunit run...)
        // I don't know what you can do with UnitTestSession
        public void ProfferConfiguration(TaskExecutorConfiguration configuration, UnitTestSession session)
        {
        }

        public string Serialize(UnitTestElement element)
        {
            return null;
        }
    }
}