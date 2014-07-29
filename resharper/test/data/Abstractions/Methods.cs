using System;

namespace Foo
{
    public class ClassWithMethods
    {
        public void NormalMethod()
        {
        }

        private void PrivateMethod()
        {
        }

        public static void StaticMethod()
        {
        }

        public void Method3(string s, int i, double d)
        {
        }

        [Custom("Foo"), Custom("Bar")]
        public void MethodWithAttributes()
        {
        }

        public T GenericMethod<T>(T t)
        {
            return default(T);
        }

        public void GenericMethod2<T1, T2>(T1 t1, T2 t2)
        {
        }

        public TResult GenericMethod3<T1, T2, TResult>(T1 t1, T2 t2)
        {
            throw new NotImplementedException();
        }

        public void WithParameters(int i, string s)
        {
        }

        public string ReturnsString()
        {
            return "foo";
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

    public abstract class AbstractClassWithMethods
    {
        public abstract void AbstractMethod();
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
}
