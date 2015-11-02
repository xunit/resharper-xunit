using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public abstract partial class XunitBaseElement : IUnitTestElement
    {
        private static readonly IUnitTestRunStrategy RunStrategy = new OutOfProcessUnitTestRunStrategy(new RemoteTaskRunnerInfo(XunitTaskRunner.RunnerId, typeof(XunitTaskRunner)));

        private IEnumerable<UnitTestElementCategory> myCategories;
        private IUnitTestElement parent;

        protected XunitBaseElement(IUnitTestElement parent, UnitTestElementId id,
                                   IEnumerable<UnitTestElementCategory> categories)
        {
            myCategories = categories;
            Parent = parent;
            Id = id;
            Children = new List<IUnitTestElement>();
            ExplicitReason = string.Empty;
            SetState(UnitTestElementState.Valid);
        }

        public void SetCategories(IEnumerable<UnitTestElementCategory> categories)
        {
            myCategories = categories;
        }

        // Simply to get around the virtual call in ctor warning
        protected void SetState(UnitTestElementState state)
        {
            State = state;
        }

        public UnitTestElementId Id { get; private set; }
        public abstract string Kind { get; }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get
            {
                if (Parent != null)
                {
                    var parentCategories = Parent.Categories;
                    if (!Equals(parentCategories, UnitTestElementCategory.Uncategorized))
                        return myCategories.Concat(Parent.Categories).Distinct();
                }
                return myCategories;
            }
        }

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

        public IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements,
                                                            IUnitTestRun run)
        {
            return GetTaskSequence(explicitElements);
        }

        public abstract IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements);

        public abstract bool Equals(IUnitTestElement other);

        public string ShortId { get { return Id.Id; } }

        protected static UnitTestElementNamespace GetNamespace(IEnumerable<string> namespaces)
        {
            return UnitTestElementNamespaceFactory.Create(namespaces);
        }
    }
}