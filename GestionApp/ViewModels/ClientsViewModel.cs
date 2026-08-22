using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GestionApp.Data;
using GestionApp.Dialogs;
using GestionApp.Models;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

public class ClientsViewModel : ViewModelBase
{
    private readonly AppDataStore _store;
    private readonly Action<string> _setStatus;

    public ICollectionView Items { get; }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { if (SetField(ref _searchText, value)) Items.Refresh(); }
    }

    private Client? _selectedItem;
    public Client? SelectedItem
    {
        get => _selectedItem;
        set { if (SetField(ref _selectedItem, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public ClientsViewModel(AppDataStore store, Action<string> setStatus)
    {
        _store = store;
        _setStatus = setStatus;

        Items = CollectionViewSource.GetDefaultView(_store.Clients);
        Items.Filter = o => o is Client c && Matches(c);

        AddCommand = new AsyncRelayCommand(AddAsync);
        EditCommand = new AsyncRelayCommand(EditAsync, () => SelectedItem != null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedItem != null);
    }

    private bool Matches(Client c)
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var q = SearchText.Trim();
        return Has(c.Name, q) || Has(c.Email, q) || Has(c.Phone, q);
    }

    private static bool Has(string? s, string q) => !string.IsNullOrEmpty(s) && s.Contains(q, StringComparison.OrdinalIgnoreCase);

    private async Task AddAsync()
    {
        var dlg = new ClientDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var ok = await _store.InsertAsync(_store.Clients, "clients", dlg.Result);
        _setStatus(ok ? "Client ajouté." : "Échec de l'ajout : " + _store.LastError);
    }

    private async Task EditAsync()
    {
        if (SelectedItem == null) return;
        var previous = SelectedItem;
        var dlg = new ClientDialog(previous) { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var ok = await _store.UpdateAsync(_store.Clients, "clients", previous, dlg.Result);
        if (ok) _store.RelinkTransactionNames();
        _setStatus(ok ? "Client modifié." : "Échec de la modification : " + _store.LastError);
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;
        if (MessageBox.Show(Application.Current.MainWindow, $"Supprimer le client « {SelectedItem.Name} » ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var ok = await _store.DeleteAsync(_store.Clients, "clients", SelectedItem);
        _setStatus(ok ? "Client supprimé." : "Échec de la suppression : " + _store.LastError);
    }
}
