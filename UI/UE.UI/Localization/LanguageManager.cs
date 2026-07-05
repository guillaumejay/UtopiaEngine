using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace UE.UI.Localization;

/// <summary>
/// Langue de l'UI : au démarrage celle du système si supportée, sinon anglais ;
/// modifiable in-app et persistée (best-effort) dans %AppData%/UtopiaEngine/language.txt.
/// Pilote CurrentUICulture, donc les .resx ET les LocalizedTexts du moteur suivent.
/// </summary>
public static class LanguageManager
{
    public static readonly string[] Supported = ["en", "fr"];
    private const string Fallback = "en";

    public static string Current { get; private set; } = Fallback;

    public static void Initialize()
    {
        string? saved = null;
        try
        {
            if (File.Exists(SettingsPath))
                saved = File.ReadAllText(SettingsPath).Trim();
        }
        catch
        {
            // Pas de système de fichiers (navigateur) : on part de la culture système.
        }
        string system = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        Apply(Normalize(saved) ?? Normalize(system) ?? Fallback);
    }

    public static void Set(string code)
    {
        Apply(Normalize(code) ?? Fallback);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, Current);
        }
        catch
        {
            // Pas de persistance : le choix vaut pour la session en cours.
        }
    }

    private static string? Normalize(string? code)
    {
        code = code?.Trim().ToLowerInvariant();
        return code is not null && Supported.Contains(code) ? code : null;
    }

    private static void Apply(string code)
    {
        Current = code;
        var culture = new CultureInfo(code);
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    private static string SettingsPath => AppData.PathFor("language.txt");
}
