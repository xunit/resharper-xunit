using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using XunitContrib.Runner.ReSharper.RemoteRunner;

#if !RESHARPER92
using UnitTestElementNamespace = JetBrains.ReSharper.UnitTestFramework.UnitTestNamespace;
#endif

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public abstract partial class XunitBaseElement : IUnitTestElement
    {
        private static readonly IUnitTestRunStrategy RunStrategy = new OutOfProcessUnitTestRunStrategy(new RemoteTaskRunnerInfo(XunitTaskRunner.RunnerId, typeof(XunitTaskRunner)));

        private IUnitTestElement parent;

        protected readonly UnitTestElementId UnitTestElementId;

        protected XunitBaseElement(IUnitTestElement parent, UnitTestElementId id,
                                   IEnumerable<UnitTestElementCategory> categories)
        {
            Parent = parent;
            UnitTestElementId = id;
            Children = new List<IUnitTestElement>();
            SetCategories(categories);
            ExplicitReason = string.Empty;
            SetState(UnitTestElementState.Valid);
        }

        public void SetCategories(IEnumerable<UnitTestElementCategory> categories)
        {
            Categories = categories;
        }

        // Simply to get around the virtual call in ctor warning
        protected void SetState(UnitTestElementState state)
        {
            State = state;
        }

        public abstract string Kind { get; }
        public IEnumerable<UnitTestElementCategory> Categories { get; private set; }
        public string ExplicitReason { get; protected set; }

        public IUnitTestElement Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                if (parent != null)
                    parent.Children.Remove(this);
                parent = value;
                if (parent != null)
                    parent.Children.Add(this);
            }
        }

        public ICollection<IUnitTestElement> Children { get; private set; }
        public string ShortName { get; protected set; }
        public bool Explicit { get { return !string.IsNullOrEmpty(ExplicitReason); } }
        public virtual UnitTestElementState State { get; set; }
        public abstract string GetPresentation(IUnitTestElement parentElement, bool full);
        public abstract UnitTestElementNamespace GetNamespace();
        public abstract UnitTestElementDisposition GetDisposition();
        public abstract IDeclaredElement GetDeclaredElement();
        public abstract IEnumerable<IProjectFile> GetProjectFiles();

        public IUnitTestRunStrategy GetRunStrategy(IHostProvider hostProvider)
        {
            return RunStrategy;
        }

        // ReSharper 9.0
        public IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements,
                                                            IUnitTestRun run)
        {
            return GetTaskSequence(explicitElements);
        }

        // ReSharper 8.2
        public IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch run)
        {
            return GetTaskSequence(explicitElements);
        }

        public abstract IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements);

        public abstract bool Equals(IUnitTestElement other);

        public string ShortId { get { return UnitTestElementId.Id; } }

        // ReSharper 8.2, but used by derived types in 9.0
        public IProject GetProject()
        {
            return UnitTestElementId.GetProject();
        }
    }
}