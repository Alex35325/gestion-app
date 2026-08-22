using System.Windows;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.Dialogs;

public partial class ClientDialog : Window
{
    private readonly Client? _editing;

    public Client Result { get; private set; } = new();

    public ClientDialog(Client? editing = null)
    {
        InitializeComponent();
        _editing = editing;

        if (editing != null)
        {
            Title = "Modifier le client";
            NameBox.Text = editing.Name;
            EmailBox.Text = editing.Email;
            PhoneBox.Text = editing.Phone;
            NotesBox.Text = editing.Notes;
        }
        else
        {
            Title = "Ajouter un client";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Le nom est obligatoire.", "Champ manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var now = Ids.NowMs();
        Result = new Client
        {
            Id = _editing?.Id ?? Ids.NewId(),
            Name = name,
            Email = EmailBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            Notes = NotesBox.Text.Trim(),
            CreatedAt = _editing?.CreatedAt ?? now,
            UpdatedAt = now
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
