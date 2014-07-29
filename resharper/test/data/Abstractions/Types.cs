using System;
using System.Collections.Generic;

namespace Foo
{
    public struct MyValueType
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomAttribute : Attribute
    {
        private readonly string value;

        public CustomAttribute(string value)
        {
            this.value = value;
        }
    }

    [Custom("Foo")]
    public abstract class BaseType
    {
        public void MethodOnBaseClass()
        {
        }
    }

    public class DerivedType : BaseType
    {
        public void PublicMethod()
        {
        }

        private void PrivateMethod()
        {
        }
    }

    [Custom("Foo"), Custom("Bar")]
    public class TypeWithInterfaces : IDisposable, IEnumerable<string>
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class GenericType<T>
    {
        public void NormalMethod(T t)
        {
        }

        public T GenericMethod()
        {
            return default(T);
        }
    }

    public class DerivedClosedGenericType : GenericType<string>
    {
    }

    public class GenericType2<T1, T2>
    {
    }

    public class DerivedClosedGenericType2 : GenericType2<string, int>
    {
    }

    public sealed class SealedType
    {
    }
}
