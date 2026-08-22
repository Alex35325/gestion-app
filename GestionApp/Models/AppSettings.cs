using System.Text.Json.Serialization;

namespace GestionApp.Models;

/// <summary>Singleton row (id=1) — company name and currency, same as the website.</summary>
public class AppSettings
{
    [JsonPropertyName("id")] public int Id { get; set; } = 1;
    [JsonPropertyName("company_name")] public string CompanyName { get; set; } = "Mon entreprise";
    [JsonPropertyName("currency")] public string Currency { get; set; } = "CAD";
}
