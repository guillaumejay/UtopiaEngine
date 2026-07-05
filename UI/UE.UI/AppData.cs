using System;
using System.IO;

namespace UE.UI;

/// <summary>Fichiers persistants de l'application (%AppData%/UtopiaEngine). Best-effort : peut ne pas exister en WASM.</summary>
internal static class AppData
{
    public static string PathFor(string fileName) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UtopiaEngine",
        fileName);
}
