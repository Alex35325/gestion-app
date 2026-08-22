using System.Text.Json.Serialization;

namespace GestionApp.Models;

public class Revenu : ITransaction
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("date")] public string Date { get; set; } = "";
    [JsonPropertyName("client_id")] public string? ClientId { get; set; }
    [JsonPropertyName("vehicule_id")] public string? VehiculeId { get; set; }
    [JsonPropertyName("produit_id")] public string? ProduitId { get; set; }
    [JsonPropertyName("montant")] public decimal Montant { get; set; }
    [JsonPropertyName("categorie")] public string Categorie { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("created_at")] public long CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public long UpdatedAt { get; set; }

    [JsonIgnore] public string ClientName { get; set; } = "";
    [JsonIgnore] public string VehiculeName { get; set; } = "";
    [JsonIgnore] public string ProduitName { get; set; } = "";

    public static readonly string[] Categories = { "Vente", "Service", "Abonnement", "Autre" };
}
