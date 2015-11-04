using System.Collections.Generic;
using JetBrains.DataFlow;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using JetBrains.UI.BindableLinq.Interfaces;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public abstract class XunitBaseElement : IUnitTestElement
    {
        protected readonly XunitServiceProvider Services;
        public readonly IClrTypeName TypeName;

        private IUnitTestElement parent;

        protected XunitBaseElement(XunitServiceProvider services, UnitTestElementId id, IClrTypeName typeName)
        {
            Services = services;
            TypeName = typeName;

            Id = id;
            Children = new BindableCollection<IUnitTestElement>(EternalLifetime.Instance, UT.Locks.ReadLock);

            ExplicitReason = string.Empty;
            Categories = UnitTestElementCategory.Uncategorized;
        }

        public UnitTestElementId Id { get; private set; }
        public ICollection<IUnitTestElement> Children { get; private set; }
        public string ShortName { get; protected set; }
        public string ExplicitReason { get; protected set; }
        public bool Explicit { get { return !string.IsNullOrEmpty(ExplicitReason); } }
        public abstract string Kind { get; }
        public IEnumerable<UnitTestElementCategory> Categories { get; set; }
        public virtual UnitTestElementState State { get; set; }
        public abstract string GetPresentation(IUnitTestElement parentElement, bool full);
        public abstract UnitTestElementDisposition GetDisposition();
        public abstract IDeclaredElement GetDeclaredElement();
        public abstract IEnumerable<IProjectFile> GetProjectFiles();

        public UnitTestElementNamespace GetNamespace()
        {
            return UnitTestElementNamespaceFactory.Create(TypeName.GetNamespaceName());
        }

        public IUnitTestElement Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                using (UT.WriteLock())
                {
                    if (parent != null)
                        parent.Children.Remove(this);
                    parent = value;
                    if (parent != null)
                        parent.Children.Add(this);
                }
            }
        }

        public IUnitTestRunStrategy GetRunStrategy(IHostProvider hostProvider)
        {
            return Services.RunStrategy;
        }

        public abstract IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestRun run);

        // ReSharper disable once UnusedMember.Global
        public abstract bool Equals(IUnitTestElement other);
    }
}