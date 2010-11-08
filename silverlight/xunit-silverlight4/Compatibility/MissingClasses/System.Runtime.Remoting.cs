using System;

// Silverlight doesn't support remoting
namespace Xunit
{
    // This class has to be public because it is used by public classes
    public class MarshalByRefObject
    {
        public virtual Object InitializeLifetimeService()
        {
            return new object();
        }
    }

    // This class can be internal - no-one else needs it
    internal class SerializableAttribute : Attribute
    {
    }

    // These classes have to be public - used by the runners. Won't cause problems, cause the
    // runtime doesn't implement them
    public interface IMessageSink
    {
        IMessageSink NextSink { get; }
        IMessage SyncProcessMessage(IMessage msg);
        IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink);
    }

    public interface IMessageCtrl
    {
        void Cancel(int msToCancel);
    }

    public interface IMessage
    {
        IDictionary Properties { get; }
    }
}