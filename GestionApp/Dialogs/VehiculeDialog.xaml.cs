using System;
using System.Globalization;
using System.Windows;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.Dialogs;

public partial class VehiculeDialog : Window
{
    private readonly Vehicule? _editing;

    public Vehicule Result { get; private set; } = new();

    public VehiculeDialog(Vehicule? editing = null)
    {
        InitializeComponent();
        _editing = editing;

        if (editing != null)
        {
            Title = "Modifier le véhicule";
            NameBox.Text = editing.Name;
            MakeBox.Text = editing.Make;
            ModelBox.Text = editing.Model;
            YearBox.Text = editing.Year;
            PlateBox.Text = editing.Plate;
            VinBox.Text = editing.Vin;
            ColorBox.Text = editing.Color;
            MileageBox.Text = editing.Mileage?.ToString(CultureInfo.InvariantCulture) ?? "";
            PurchaseDateBox.SelectedDate = DateTime.TryParse(editing.PurchaseDate, out var d) ? d : null;
            NotesBox.Text = editing.Notes;
        }
        else
        {
            Title = "Ajouter un véhicule";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Le nom / dossier est obligatoire.", "Champ manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        decimal? mileage = null;
        if (!string.IsNullOrWhiteSpace(MileageBox.Text))
        {
            if (!decimal.TryParse(MileageBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var m)
                && !decimal.TryParse(MileageBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out m))
            {
                MessageBox.Show(this, "Le kilométrage doit être un nombre.", "Champ invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            mileage = m;
        }

        var now = Ids.NowMs();
        Result = new Vehicule
        {
            Id = _editing?.Id ?? Ids.NewId(),
            Name = name,
            Make = MakeBox.Text.Trim(),
            Model = ModelBox.Text.Trim(),
            Year = YearBox.Text.Trim(),
            Plate = PlateBox.Text.Trim(),
            Vin = VinBox.Text.Trim(),
            Color = ColorBox.Text.Trim(),
            Mileage = mileage,
            PurchaseDate = PurchaseDateBox.SelectedDate?.ToString("yyyy-MM-dd"),
            Notes = NotesBox.Text.Trim(),
            CreatedAt = _editing?.CreatedAt ?? now,
            UpdatedAt = now
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
