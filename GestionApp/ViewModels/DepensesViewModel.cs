using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GestionApp.Data;
using GestionApp.Dialogs;
using GestionApp.Models;

namespace GestionApp.ViewModels;

public class DepensesViewModel : TransactionsViewModelBase<Depense>
{
    protected override string Table => "depenses";
    protected override ObservableCollection<Depense> Source => Store.Depenses;

    public DepensesViewModel(AppDataStore store, Func<Depense, bool> scopeFilter, string title, Action<string> setStatus)
        : base(store, scopeFilter, title, "Rechercher par client, véhicule, produit, catégorie ou description...", setStatus)
    {
    }

    protected override Depense? ShowDialog(Depense? editing)
    {
        var dlg = new DepenseDialog(Store.Clients.ToList(), Store.Vehicules.ToList(), Store.Inventaire.ToList(), editing)
        {
            Owner = Application.Current.MainWindow
        };
        return dlg.ShowDialog() == true ? dlg.Result : null;
    }
}
