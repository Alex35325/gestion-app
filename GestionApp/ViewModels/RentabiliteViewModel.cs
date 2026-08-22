using System.Collections.ObjectModel;
using System.Linq;
using GestionApp.Data;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

public enum RentabiliteType { Vehicule, Produit }

public class RentabiliteRow
{
    public string Name { get; set; } = "";
    public decimal Revenus { get; set; }
    public decimal Depenses { get; set; }
    public decimal Profit => Revenus - Depenses;
}

/// <summary>Profit by véhicule or by produit — sums revenus/depenses tagged
/// to each one, same idea as the website's Rentabilité tab.</summary>
public class RentabiliteViewModel : ViewModelBase
{
    private readonly AppDataStore _store;

    private RentabiliteType _type = RentabiliteType.Vehicule;
    public RentabiliteType Type
    {
        get => _type;
        set { if (SetField(ref _type, value)) Recompute(); }
    }

    public bool IsVehiculeType
    {
        get => Type == RentabiliteType.Vehicule;
        set { if (value) Type = RentabiliteType.Vehicule; }
    }

    public bool IsProduitType
    {
        get => Type == RentabiliteType.Produit;
        set { if (value) Type = RentabiliteType.Produit; }
    }

    public ObservableCollection<RentabiliteRow> Rows { get; } = new();

    public RentabiliteViewModel(AppDataStore store)
    {
        _store = store;
        Recompute();
    }

    private void Recompute()
    {
        Rows.Clear();

        if (Type == RentabiliteType.Vehicule)
        {
            foreach (var v in _store.Vehicules)
            {
                Rows.Add(new RentabiliteRow
                {
                    Name = v.Name,
                    Revenus = _store.Revenus.Where(r => r.VehiculeId == v.Id).Sum(r => r.Montant),
                    Depenses = _store.Depenses.Where(d => d.VehiculeId == v.Id).Sum(d => d.Montant)
                });
            }
        }
        else
        {
            foreach (var p in _store.Inventaire)
            {
                Rows.Add(new RentabiliteRow
                {
                    Name = p.Name,
                    Revenus = _store.Revenus.Where(r => r.ProduitId == p.Id).Sum(r => r.Montant),
                    Depenses = _store.Depenses.Where(d => d.ProduitId == p.Id).Sum(d => d.Montant)
                });
            }
        }

        OnPropertyChanged(nameof(IsVehiculeType));
        OnPropertyChanged(nameof(IsProduitType));
    }
}
