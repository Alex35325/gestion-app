using System;
using System.Collections.Generic;
using System.Windows;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.Dialogs;

public partial class MaintenanceDialog : Window
{
    private readonly Maintenance? _editing;

    public Maintenance Result { get; private set; } = new();

    public MaintenanceDialog(List<Vehicule> vehicules, Maintenance? editing = null)
    {
        InitializeComponent();
        _editing = editing;

        VehiculeCombo.ItemsSource = vehicules;
        TypeCombo.ItemsSource = Maintenance.Types;

        if (editing != null)
        {
            Title = "Modifier l'entretien";
            VehiculeCombo.SelectedValue = editing.VehiculeId;
            TypeCombo.SelectedItem = editing.Type;
            DateBox.SelectedDate = DateTime.TryParse(editing.Date, out var d) ? d : null;
            NextDueDateBox.SelectedDate = DateTime.TryParse(editing.NextDueDate, out var nd) ? nd : null;
            NotesBox.Text = editing.Notes;
        }
        else
        {
            Title = "Ajouter un entretien";
            if (vehicules.Count > 0) VehiculeCombo.SelectedIndex = 0;
            TypeCombo.SelectedIndex = 0;
            DateBox.SelectedDate = DateTime.Now;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var vehiculeId = VehiculeCombo.SelectedValue as string;
        if (string.IsNullOrEmpty(vehiculeId))
        {
            MessageBox.Show(this, "Le véhicule est obligatoire.", "Champ manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var now = Ids.NowMs();
        Result = new Maintenance
        {
            Id = _editing?.Id ?? Ids.NewId(),
            VehiculeId = vehiculeId,
            Type = TypeCombo.SelectedItem as string ?? Maintenance.Types[0],
            Date = DateBox.SelectedDate?.ToString("yyyy-MM-dd"),
            NextDueDate = NextDueDateBox.SelectedDate?.ToString("yyyy-MM-dd"),
            Notes = NotesBox.Text.Trim(),
            CreatedAt = _editing?.CreatedAt ?? now,
            UpdatedAt = now
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
