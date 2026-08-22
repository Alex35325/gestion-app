using GestionApp.Models;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

/// <summary>A vehicule paired with its maintenance status counts, recomputed
/// whenever VehiculesInfoViewModel rebuilds its card list.</summary>
public class VehiculeCard : ViewModelBase
{
    public Vehicule Vehicule { get; }
    public int LateCount { get; }
    public int SoonCount { get; }
    public int OkCount { get; }

    public VehiculeCard(Vehicule vehicule, int lateCount, int soonCount, int okCount)
    {
        Vehicule = vehicule;
        LateCount = lateCount;
        SoonCount = soonCount;
        OkCount = okCount;
    }
}
