using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp;

/// <summary>
/// Talks to the same Supabase tables (clients/revenus/depenses) as the
/// gestion-app website, so data stays shared between the web app and this
/// desktop app. Each table loads independently and every write uses an
/// optimistic-update-with-rollback pattern, matching gestion-app.html.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly CultureInfo Fr = new("fr-CA");

    private readonly SupabaseService _sb = new();

    private readonly ObservableCollection<Client> _clients = new();
    private readonly ObservableCollection<Revenu> _revenus = new();
    private readonly ObservableCollection<Depense> _depenses = new();

    private ICollectionView _clientsView = null!;
    private ICollectionView _revenusView = null!;
    private ICollectionView _depensesView = null!;

    public MainWindow()
    {
        InitializeComponent();

        _clientsView = CollectionViewSource.GetDefaultView(_clients);
        _revenusView = CollectionViewSource.GetDefaultView(_revenus);
        _depensesView = CollectionViewSource.GetDefaultView(_depenses);
        ClientsGrid.ItemsSource = _clientsView;
        RevenusGrid.ItemsSource = _revenusView;
        DepensesGrid.ItemsSource = _depensesView;

        _clientsView.Filter = o => o is Client c && Matches(ClientSearchBox.Text, c.Name, c.Email, c.Phone);
        _revenusView.Filter = o => o is Revenu r && Matches(RevenuSearchBox.Text, r.ClientName, r.Categorie, r.Description);
        _depensesView.Filter = o => o is Depense d && Matches(DepenseSearchBox.Text, d.ClientName, d.Categorie, d.Description);

        Loaded += async (_, _) => await LoadAllAsync();
    }

    private static bool Matches(string? query, params string?[] fields)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;
        var q = query.Trim();
        return fields.Any(f => !string.IsNullOrEmpty(f) && f.Contains(q, StringComparison.OrdinalIgnoreCase));
    }

    private async Task LoadAllAsync()
    {
        StatusText.Text = "Chargement...";
        var errors = 0;

        try
        {
            var clients = await _sb.GetAllAsync<Client>("clients");
            _clients.Clear();
            foreach (var c in clients.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)) _clients.Add(c);
        }
        catch (Exception ex)
        {
            errors++;
            StatusText.Text = "Erreur de chargement des clients : " + ex.Message;
        }

        try
        {
            var revenus = await _sb.GetAllAsync<Revenu>("revenus");
            _revenus.Clear();
            foreach (var r in revenus.OrderByDescending(r => r.Date))
            {
                r.ClientName = ClientName(r.ClientId);
                _revenus.Add(r);
            }
        }
        catch (Exception ex)
        {
            errors++;
            StatusText.Text = "Erreur de chargement des revenus : " + ex.Message;
        }

        try
        {
            var depenses = await _sb.GetAllAsync<Depense>("depenses");
            _depenses.Clear();
            foreach (var d in depenses.OrderByDescending(d => d.Date))
            {
                d.ClientName = ClientName(d.ClientId);
                _depenses.Add(d);
            }
        }
        catch (Exception ex)
        {
            errors++;
            StatusText.Text = "Erreur de chargement des dépenses : " + ex.Message;
        }

        RefreshTotals();
        if (errors == 0)
            StatusText.Text = $"Prêt — {_clients.Count} clients, {_revenus.Count} revenus, {_depenses.Count} dépenses";
    }

    private string ClientName(string? clientId)
    {
        if (string.IsNullOrEmpty(clientId)) return "";
        return _clients.FirstOrDefault(c => c.Id == clientId)?.Name ?? "";
    }

    private void RefreshTotals()
    {
        RevenuTotalText.Text = "Total : " + _revenus.Sum(r => r.Montant).ToString("C", Fr);
        DepenseTotalText.Text = "Total : " + _depenses.Sum(d => d.Montant).ToString("C", Fr);
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAllAsync();

    // ---------- search ----------
    private void ClientSearchBox_TextChanged(object sender, TextChangedEventArgs e) => _clientsView.Refresh();
    private void RevenuSearchBox_TextChanged(object sender, TextChangedEventArgs e) => _revenusView.Refresh();
    private void DepenseSearchBox_TextChanged(object sender, TextChangedEventArgs e) => _depensesView.Refresh();

    // ---------- clients CRUD ----------
    private async void AddClient_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ClientDialog { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var client = dlg.Result;
        _clients.Add(client);
        try { await _sb.InsertAsync("clients", client); StatusText.Text = "Client ajouté."; }
        catch (Exception ex) { _clients.Remove(client); StatusText.Text = "Échec de l'ajout : " + ex.Message; }
    }

    private async void EditClient_Click(object sender, RoutedEventArgs e)
    {
        if (ClientsGrid.SelectedItem is not Client selected) { StatusText.Text = "Sélectionnez un client."; return; }
        var dlg = new ClientDialog(selected) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var updated = dlg.Result;

        var index = _clients.IndexOf(selected);
        _clients[index] = updated;
        try
        {
            await _sb.UpdateAsync("clients", updated.Id, updated);
            StatusText.Text = "Client modifié.";
            SyncClientNames();
        }
        catch (Exception ex)
        {
            _clients[index] = selected;
            StatusText.Text = "Échec de la modification : " + ex.Message;
        }
    }

    private async void DeleteClient_Click(object sender, RoutedEventArgs e)
    {
        if (ClientsGrid.SelectedItem is not Client selected) { StatusText.Text = "Sélectionnez un client."; return; }
        if (MessageBox.Show(this, $"Supprimer le client « {selected.Name} » ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var index = _clients.IndexOf(selected);
        _clients.RemoveAt(index);
        try { await _sb.DeleteAsync("clients", selected.Id); StatusText.Text = "Client supprimé."; }
        catch (Exception ex) { _clients.Insert(index, selected); StatusText.Text = "Échec de la suppression : " + ex.Message; }
    }

    private void ClientsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => EditClient_Click(sender, e);

    // ---------- revenus CRUD ----------
    private async void AddRevenu_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new RevenuDialog(_clients.ToList()) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var revenu = dlg.Result;
        revenu.ClientName = ClientName(revenu.ClientId);
        _revenus.Insert(0, revenu);
        RefreshTotals();
        try { await _sb.InsertAsync("revenus", revenu); StatusText.Text = "Revenu ajouté."; }
        catch (Exception ex) { _revenus.Remove(revenu); RefreshTotals(); StatusText.Text = "Échec de l'ajout : " + ex.Message; }
    }

    private async void EditRevenu_Click(object sender, RoutedEventArgs e)
    {
        if (RevenusGrid.SelectedItem is not Revenu selected) { StatusText.Text = "Sélectionnez un revenu."; return; }
        var dlg = new RevenuDialog(_clients.ToList(), selected) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var updated = dlg.Result;
        updated.ClientName = ClientName(updated.ClientId);

        var index = _revenus.IndexOf(selected);
        _revenus[index] = updated;
        RefreshTotals();
        try { await _sb.UpdateAsync("revenus", updated.Id, updated); StatusText.Text = "Revenu modifié."; }
        catch (Exception ex) { _revenus[index] = selected; RefreshTotals(); StatusText.Text = "Échec de la modification : " + ex.Message; }
    }

    private async void DeleteRevenu_Click(object sender, RoutedEventArgs e)
    {
        if (RevenusGrid.SelectedItem is not Revenu selected) { StatusText.Text = "Sélectionnez un revenu."; return; }
        if (MessageBox.Show(this, "Supprimer ce revenu ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var index = _revenus.IndexOf(selected);
        _revenus.RemoveAt(index);
        RefreshTotals();
        try { await _sb.DeleteAsync("revenus", selected.Id); StatusText.Text = "Revenu supprimé."; }
        catch (Exception ex) { _revenus.Insert(index, selected); RefreshTotals(); StatusText.Text = "Échec de la suppression : " + ex.Message; }
    }

    private void RevenusGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => EditRevenu_Click(sender, e);

    // ---------- depenses CRUD ----------
    private async void AddDepense_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new DepenseDialog(_clients.ToList()) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var depense = dlg.Result;
        depense.ClientName = ClientName(depense.ClientId);
        _depenses.Insert(0, depense);
        RefreshTotals();
        try { await _sb.InsertAsync("depenses", depense); StatusText.Text = "Dépense ajoutée."; }
        catch (Exception ex) { _depenses.Remove(depense); RefreshTotals(); StatusText.Text = "Échec de l'ajout : " + ex.Message; }
    }

    private async void EditDepense_Click(object sender, RoutedEventArgs e)
    {
        if (DepensesGrid.SelectedItem is not Depense selected) { StatusText.Text = "Sélectionnez une dépense."; return; }
        var dlg = new DepenseDialog(_clients.ToList(), selected) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var updated = dlg.Result;
        updated.ClientName = ClientName(updated.ClientId);

        var index = _depenses.IndexOf(selected);
        _depenses[index] = updated;
        RefreshTotals();
        try { await _sb.UpdateAsync("depenses", updated.Id, updated); StatusText.Text = "Dépense modifiée."; }
        catch (Exception ex) { _depenses[index] = selected; RefreshTotals(); StatusText.Text = "Échec de la modification : " + ex.Message; }
    }

    private async void DeleteDepense_Click(object sender, RoutedEventArgs e)
    {
        if (DepensesGrid.SelectedItem is not Depense selected) { StatusText.Text = "Sélectionnez une dépense."; return; }
        if (MessageBox.Show(this, "Supprimer cette dépense ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var index = _depenses.IndexOf(selected);
        _depenses.RemoveAt(index);
        RefreshTotals();
        try { await _sb.DeleteAsync("depenses", selected.Id); StatusText.Text = "Dépense supprimée."; }
        catch (Exception ex) { _depenses.Insert(index, selected); RefreshTotals(); StatusText.Text = "Échec de la suppression : " + ex.Message; }
    }

    private void DepensesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => EditDepense_Click(sender, e);

    private void SyncClientNames()
    {
        foreach (var r in _revenus) r.ClientName = ClientName(r.ClientId);
        foreach (var d in _depenses) d.ClientName = ClientName(d.ClientId);
        _revenusView.Refresh();
        _depensesView.Refresh();
    }
}
