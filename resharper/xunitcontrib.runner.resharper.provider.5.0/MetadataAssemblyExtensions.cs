using System.Collections.Generic;
using JetBrains.Metadata.Reader.API;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class MetadataAssemblyExtensions
    {
        internal static void ProcessExportedTypes(this IMetadataAssembly assembly, IMetadataTypeProcessor processor)
        {
            foreach (var metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
            {
                processor.ProcessTypeInfo(metadataTypeInfo);
            }
        }

        // ReSharper's IMetadataAssembly.GetExportedTypes always seems to return an empty list, so
        // let's roll our own. MSDN says that Assembly.GetExportTypes is looking for "The only types
        // visible outside an assembly are public types and public types nested within other public types."
        // TODO: It might be nice to randomise this list:
        // However, this returns items in alphabetical ordering. Assembly.GetExportedTypes returns back in
        // the order in which classes are compiled (so the order in which their files appear in the msbuild file!)
        // with dependencies appearing first. 
        private static IEnumerable<IMetadataTypeInfo> GetExportedTypes(IEnumerable<IMetadataTypeInfo> types)
        {
            foreach (var type in types ?? new IMetadataTypeInfo[0])
            {
                if (!IsPublic(type)) continue;

                foreach (var nestedType in GetExportedTypes(type.GetNestedTypes()))
                {
                    if (IsPublic(nestedType))
                        yield return nestedType;
                }

                yield return type;
            }
        }

        private static bool IsPublic(IMetadataTypeInfo type)
        {
            // Hmmm. This seems a little odd. Resharper reports public nested types with IsNestedPublic,
            // while IsPublic is false
            return (type.IsNested && type.IsNestedPublic) || type.IsPublic;
        }
    }
}