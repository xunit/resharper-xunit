namespace Xunit
{
    // AssemblyName is missing the static GetAssemblyName. Make xunit use our version of AssemblyName. Keep this internal!
    internal class AssemblyName
    {
        public static System.Reflection.AssemblyName GetAssemblyName(string assemblyFilename)
        {
            return new System.Reflection.AssemblyName(assemblyFilename);
        }
    }
}