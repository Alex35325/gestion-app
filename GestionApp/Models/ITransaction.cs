namespace GestionApp.Models;

/// <summary>
/// Shared shape of Revenu and Depense — lets the Clients/Véhicules/Inventaire
/// sub-tabs (Revenus and Dépenses each shown three ways, scoped to a
/// different parent) share one generic list+search+CRUD view model instead
/// of six near-identical copies.
/// </summary>
public interface ITransaction : IEntity
{
    string Date { get; set; }
    string? ClientId { get; set; }
    string? VehiculeId { get; set; }
    string? ProduitId { get; set; }
    decimal Montant { get; set; }
    string Categorie { get; set; }
    string Description { get; set; }

    string ClientName { get; set; }
    string VehiculeName { get; set; }
    string ProduitName { get; set; }
}
