using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public abstract partial class XunitBaseElement : IUnitTestElement
    {
        private readonly ProjectModelElementEnvoy projectModelElementEnvoy;
        private IUnitTestElement parent;

        protected XunitBaseElement(IUnitTestProvider provider, IUnitTestElement parent, string id,
                                   ProjectModelElementEnvoy projectModelElementEnvoy,
                                   IEnumerable<UnitTestElementCategory> categories)
        {
            Provider = provider;
            Parent = parent;
            Id = id;
            this.projectModelElementEnvoy = projectModelElementEnvoy;
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
        public string Id { get; private set; }
        public IUnitTestProvider Provider { get; private set; }

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

        public IProject GetProject()
        {
            return projectModelElementEnvoy.GetValidProjectElement() as IProject;
        }

        // ReSharper 6.1
        public string GetPresentation()
        {
            return GetPresentation(null);
        }

        public abstract string GetPresentation(IUnitTestElement parentElement);
        public abstract UnitTestNamespace GetNamespace();
        public abstract UnitTestElementDisposition GetDisposition();
        public abstract IDeclaredElement GetDeclaredElement();
        public abstract IEnumerable<IProjectFile> GetProjectFiles();

        // ReSharper 6.1
        public IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            return GetTaskSequence(explicitElements, null);
        }

        public abstract IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch);

        public abstract bool Equals(IUnitTestElement other);
    }
}