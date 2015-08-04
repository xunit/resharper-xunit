using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tasks
{
    public abstract class XunitTaskBase : RemoteTask
    {
        protected XunitTaskBase()
            : base(XunitTaskRunner.RunnerId)
        {
        }

        protected XunitTaskBase(XmlElement element)
            : base(element)
        {
        }
    }

    public abstract class DynamicElementXunitTaskBase : XunitTaskBase
    {
        protected DynamicElementXunitTaskBase(string uncollapsedParentTaskId)
        {
            UncollapsedParentTaskId = uncollapsedParentTaskId;
        }

        protected DynamicElementXunitTaskBase(XmlElement element)
            : base(element)
        {
            // Get the parent task ID from the runner
            UncollapsedParentTaskId = GetXmlAttribute(element, AttributeNames.ParentId);
        }

        /// <summary>
        /// The ID of the parent task. Unreliable!
        /// </summary>
        /// <remarks>
        /// Required in order to find the parent element of a newly created task.
        /// Should really only be set when sending from the runner to the provider,
        /// not when creating tasks in <see cref="IUnitTestElement.GetTaskSequence"/>.
        /// The problem is that the tasks returned from <see cref="IUnitTestElement.GetTaskSequence"/>
        /// are collapsed based on equality, and each task would have a different
        /// parent task ID. Also, we'll end up with tasks in the runner that have
        /// a parent task ID that's doesn't match any of the tasks it knows about.
        /// <para>
        /// So, just like <see cref="RemoteTask.Id"/>, this property should not be
        /// included in the equality comparers.
        /// </para>
        /// </remarks>
        public string UncollapsedParentTaskId { get; private set; }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.ParentId, UncollapsedParentTaskId);
        }
    }
}