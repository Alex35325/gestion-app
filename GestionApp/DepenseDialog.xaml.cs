using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp;

public partial class DepenseDialog : Window
{
    private static readonly string[] Categories = { "Loyer", "Fournitures", "Salaires", "Marketing", "Logiciels", "Autre" };

    private readonly Depense? _editing;

    public Depense Result { get; private set; } = new();

    public DepenseDialog(List<Client> clients, Depense? editing = null)
    {
        InitializeComponent();
        _editing = editing;

        CategorieCombo.ItemsSource = Categories;

        var clientChoices = new List<Client> { new() { Id = "", Name = "Aucun client" } };
        clientChoices.AddRange(clients);
        ClientCombo.ItemsSource = clientChoices;

        if (editing != null)
        {
            Title = "Modifier la dépense";
            DatePickerBox.SelectedDate = DateTime.TryParse(editing.Date, out var d) ? d : DateTime.Now;
            ClientCombo.SelectedValue = editing.ClientId ?? "";
            MontantBox.Text = editing.Montant.ToString(CultureInfo.InvariantCulture);
            CategorieCombo.SelectedItem = editing.Categorie;
            DescriptionBox.Text = editing.Description;
        }
        else
        {
            Title = "Ajouter une dépense";
            DatePickerBox.SelectedDate = DateTime.Now;
            ClientCombo.SelectedIndex = 0;
            CategorieCombo.SelectedIndex = 0;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DatePickerBox.SelectedDate == null)
        {
            MessageBox.Show(this, "La date est obligatoire.", "Champ manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(MontantBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var montant)
            && !decimal.TryParse(MontantBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out montant))
        {
            MessageBox.Show(this, "Le montant doit être un nombre.", "Champ invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var clientId = ClientCombo.SelectedValue as string;
        var now = Ids.NowMs();
        Result = new Depense
        {
            Id = _editing?.Id ?? Ids.NewId(),
            Date = DatePickerBox.SelectedDate.Value.ToString("yyyy-MM-dd"),
            ClientId = string.IsNullOrEmpty(clientId) ? null : clientId,
            VehiculeId = _editing?.VehiculeId,
            ProduitId = _editing?.ProduitId,
            Montant = montant,
            Categorie = CategorieCombo.SelectedItem as string ?? Categories[0],
            Description = DescriptionBox.Text.Trim(),
            CreatedAt = _editing?.CreatedAt ?? now,
            UpdatedAt = now
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
