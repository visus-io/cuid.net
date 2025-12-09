namespace Visus.Cuid.Tests;

using System.Runtime.CompilerServices;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
    }
}
