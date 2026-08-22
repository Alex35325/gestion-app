using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GestionApp.Data;
using GestionApp.Dialogs;
using GestionApp.Models;

namespace GestionApp.ViewModels;

public class RevenusViewModel : TransactionsViewModelBase<Revenu>
{
    protected override string Table => "revenus";
    protected override ObservableCollection<Revenu> Source => Store.Revenus;

    public RevenusViewModel(AppDataStore store, Func<Revenu, bool> scopeFilter, string title, Action<string> setStatus)
        : base(store, scopeFilter, title, "Rechercher par client, véhicule, produit, catégorie ou description...", setStatus)
    {
    }

    protected override Revenu? ShowDialog(Revenu? editing)
    {
        var dlg = new RevenuDialog(Store.Clients.ToList(), Store.Vehicules.ToList(), Store.Inventaire.ToList(), editing)
        {
            Owner = Application.Current.MainWindow
        };
        return dlg.ShowDialog() == true ? dlg.Result : null;
    }
}
