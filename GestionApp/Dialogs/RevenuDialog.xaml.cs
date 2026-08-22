using System;
using System.Collections.Generic;
using System.Globalization;
using GestionApp.Models;
using GestionApp.Services;
using System.Windows;

namespace GestionApp.Dialogs;

public partial class RevenuDialog : Window
{
    private readonly Revenu? _editing;

    public Revenu Result { get; private set; } = new();

    public RevenuDialog(List<Client> clients, List<Vehicule> vehicules, List<InventaireItem> produits, Revenu? editing = null)
    {
        InitializeComponent();
        _editing = editing;

        CategorieCombo.ItemsSource = Revenu.Categories;

        var clientChoices = new List<Client> { new() { Id = "", Name = "Aucun client" } };
        clientChoices.AddRange(clients);
        ClientCombo.ItemsSource = clientChoices;

        var vehiculeChoices = new List<Vehicule> { new() { Id = "", Name = "Aucun véhicule" } };
        vehiculeChoices.AddRange(vehicules);
        VehiculeCombo.ItemsSource = vehiculeChoices;

        var produitChoices = new List<InventaireItem> { new() { Id = "", Name = "Aucun produit" } };
        produitChoices.AddRange(produits);
        ProduitCombo.ItemsSource = produitChoices;

        if (editing != null)
        {
            Title = "Modifier le revenu";
            DatePickerBox.SelectedDate = DateTime.TryParse(editing.Date, out var d) ? d : DateTime.Now;
            ClientCombo.SelectedValue = editing.ClientId ?? "";
            VehiculeCombo.SelectedValue = editing.VehiculeId ?? "";
            ProduitCombo.SelectedValue = editing.ProduitId ?? "";
            MontantBox.Text = editing.Montant.ToString(CultureInfo.InvariantCulture);
            CategorieCombo.SelectedItem = editing.Categorie;
            DescriptionBox.Text = editing.Description;
        }
        else
        {
            Title = "Ajouter un revenu";
            DatePickerBox.SelectedDate = DateTime.Now;
            ClientCombo.SelectedIndex = 0;
            VehiculeCombo.SelectedIndex = 0;
            ProduitCombo.SelectedIndex = 0;
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
        var vehiculeId = VehiculeCombo.SelectedValue as string;
        var produitId = ProduitCombo.SelectedValue as string;
        var now = Ids.NowMs();
        Result = new Revenu
        {
            Id = _editing?.Id ?? Ids.NewId(),
            Date = DatePickerBox.SelectedDate.Value.ToString("yyyy-MM-dd"),
            ClientId = string.IsNullOrEmpty(clientId) ? null : clientId,
            VehiculeId = string.IsNullOrEmpty(vehiculeId) ? null : vehiculeId,
            ProduitId = string.IsNullOrEmpty(produitId) ? null : produitId,
            Montant = montant,
            Categorie = CategorieCombo.SelectedItem as string ?? Revenu.Categories[0],
            Description = DescriptionBox.Text.Trim(),
            CreatedAt = _editing?.CreatedAt ?? now,
            UpdatedAt = now
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
