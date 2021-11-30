using System.Reflection;

namespace Apache.Openwhisk.Runtime.Minimal
{
    public class Run
    {
        public Run(Type type, MethodInfo method, ConstructorInfo constructor, bool awaitableMethod)
        {
            Type = type;
            Method = method;
            Constructor = constructor;
            AwaitableMethod = awaitableMethod;
        }

        public Type Type { get; }
        public MethodInfo Method { get; }
        public ConstructorInfo Constructor { get; }
        public bool AwaitableMethod { get; }
    }
}
