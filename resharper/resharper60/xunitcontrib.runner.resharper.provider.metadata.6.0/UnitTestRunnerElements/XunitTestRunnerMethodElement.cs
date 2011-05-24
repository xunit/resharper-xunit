using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.UnitTestRunnerElements
{
    public class XunitTestRunnerMethodElement : IUnitTestRunnerElement
    {
        private XunitTestRunnerClassElement testClass;

        public XunitTestRunnerMethodElement(IUnitTestRunnerProvider provider, XunitTestRunnerClassElement testClass, string id, string typeName, string methodName, string skipReason)
        {
            Provider = provider;
            Parent = testClass;
            Id = id;
            TypeName = typeName;
            ShortName = methodName;
            MethodName = methodName;
            SkipReason = skipReason;
            State = UnitTestElementState.Valid;
        }

        public bool Equals(IUnitTestRunnerElement other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() == GetType())
            {
                return other.Provider == Provider && other.Parent == Parent && other.Id == Id
                    && other.ShortName == ShortName;
            }
            return false;
        }

        // TODO: Update comment
        //
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
        // task in order, so in the example, we load the assembly once, then we load the class once,
        // and then we have multiple tasks for each running test method. If there are multiple classes
        // run, then multiple class lists will be created and collapsed under a single assembly node.
        // If multiple assemblies are run, then there will be multiple top level assembly nodes, each
        // with one or more unique class nodes, each with one or more unique test method nodes
        // If you explicitly run a test method from its menu, then it will appear in the list of
        // explicit elements.
        // ReSharper provides an AssemblyLoadTask that can be used to load the assembly via reflection.
        // In the remote runner process, this task is serviced by a runner that will create an app domain
        // shadow copy the files (which we can affect via ProfferConfiguration) and then cause the rest
        // of the nodes (e.g. the classes + methods) to get executed (by serializing the nodes, containing
        // the remote tasks from these lists, over into the new app domain). This is the default way the
        // nunit and mstest providers work.
        public IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestRunnerElement> explicitElements)
        {
            explicitElements = explicitElements.ToList();

            return new List<UnitTestTask>
                       {
                           new UnitTestTask(null, new XunitTestAssemblyTask(testClass.AssemblyLocation)),
                           new UnitTestTask(testClass, new XunitTestClassTask(testClass.AssemblyLocation, TypeName, explicitElements.Contains(testClass))),
                           new UnitTestTask(this, new XunitTestMethodTask(testClass.AssemblyLocation, TypeName, ShortName, explicitElements.Contains(this)))
                       };
        }

        public string Id { get; private set; }
        public IUnitTestRunnerProvider Provider { get; private set; }
        public IUnitTestRunnerElement Parent
        {
            get { return testClass; }
            set
            {
                if (value != testClass)
                {
                    if (testClass != null)
                        testClass.Children.Remove(this);
                    testClass = (XunitTestRunnerClassElement) value;
                    if (testClass != null)
                        testClass.Children.Add(this);
                }
            }
        }

        public ICollection<IUnitTestRunnerElement> Children
        {
            get { return Enumerable.Empty<IUnitTestRunnerElement>().ToList(); }
        }

        public string ShortName { get; private set; }
        public bool Explicit { get { return !string.IsNullOrEmpty(SkipReason); } }
        public UnitTestElementState State { get; set; }

        public string TypeName { get; private set; }
        public string MethodName { get; private set; }
        public string SkipReason { get; private set; }
    }
}
