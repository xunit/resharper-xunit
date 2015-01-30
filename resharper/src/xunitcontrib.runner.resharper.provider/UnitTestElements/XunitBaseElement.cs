using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public abstract partial class XunitBaseElement : IUnitTestElement
    {
        private IUnitTestElement parent;

        protected XunitBaseElement(IUnitTestElement parent, UnitTestElementId id,
                                   IEnumerable<UnitTestElementCategory> categories)
        {
            Parent = parent;
            Id = id;
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
        public UnitTestElementId Id { get; private set; }

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
        public abstract UnitTestNamespace GetNamespace();
        public abstract UnitTestElementDisposition GetDisposition();
        public abstract IDeclaredElement GetDeclaredElement();
        public abstract IEnumerable<IProjectFile> GetProjectFiles();

        public abstract IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestRun run);

        public abstract bool Equals(IUnitTestElement other);

        // TODO: Make protected
        public IProject GetProject()
        {
            return Id.GetProject();
        }
    }
}