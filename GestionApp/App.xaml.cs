using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace GestionApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionApp", "erreurs.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            LogError(args.Exception);
            MessageBox.Show(
                "Une erreur inattendue est survenue et a été enregistrée dans " + LogPath + ".\n\n" + args.Exception.Message,
                "Gestion App — erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }

    private static void LogError(Exception ex)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"{DateTime.Now:O}\n{ex}\n\n");
        }
        catch
        {
            // Logging must never itself crash the app.
        }
    }
}
