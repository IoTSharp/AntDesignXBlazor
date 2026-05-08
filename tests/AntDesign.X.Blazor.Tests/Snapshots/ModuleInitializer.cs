using System.Runtime.CompilerServices;
using VerifyTests;

namespace AntDesign.X.Tests.Snapshots;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        Environment.SetEnvironmentVariable("DiffEngine_Disabled", "true");
        Environment.SetEnvironmentVariable("Verify_DisableClipboard", "true");
        VerifierSettings.UseUtf8NoBom();
    }
}
