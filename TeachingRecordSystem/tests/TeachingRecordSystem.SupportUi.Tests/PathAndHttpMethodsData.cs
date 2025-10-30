using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace TeachingRecordSystem.SupportUi.Tests;

public class PathAndHttpMethodsData(string[] paths, TestHttpMethods methods) : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        return new(Impl().AsReadOnly());

        IEnumerable<ITheoryDataRow> Impl()
        {
            foreach (var method in methods.SplitTestMethods())
            {
                foreach (var path in paths)
                {
                    yield return new TheoryDataRow(path, method);
                }
            }
        }
    }

    public override bool SupportsDiscoveryEnumeration() => true;
}
