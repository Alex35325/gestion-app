using System;
using System.IO;
using System.Text.Json;
using GestionApp.Models;

namespace GestionApp.Services;

public static class PreferencesService
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionApp", "preferences.json");

    public static AppPreferences Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new AppPreferences();
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppPreferences>(json) ?? new AppPreferences();
        }
        catch
        {
            return new AppPreferences();
        }
    }

    public static void Save(AppPreferences prefs)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var json = JsonSerializer.Serialize(prefs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // Preferences are a nice-to-have; a failed save must never crash the app.
        }
    }
}
