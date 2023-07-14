using System.Reflection;
using Fixie;

namespace TeachingRecordSystem.TestFramework;

internal class Discovery : IDiscovery
{
    public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses) =>
        concreteClasses.Where(x => x.GetCustomAttribute<TestClassAttribute>(inherit: true) is not null);

    public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods) =>
        publicMethods.Where(x => x.GetCustomAttribute<TestAttribute>() is not null);
}
