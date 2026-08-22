using System;
using System.Text.Json.Serialization;

namespace GestionApp.Models;

public enum MaintenanceStatus { Ok, Soon, Late }

public class Maintenance : IEntity
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("vehicule_id")] public string VehiculeId { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "Autre";
    [JsonPropertyName("date")] public string? Date { get; set; }
    [JsonPropertyName("next_due_date")] public string? NextDueDate { get; set; }
    [JsonPropertyName("notes")] public string Notes { get; set; } = "";
    [JsonPropertyName("created_at")] public long CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public long UpdatedAt { get; set; }

    [JsonIgnore] public string VehiculeName { get; set; } = "";

    public static readonly string[] Types =
        { "Vidange d'huile", "Freins", "Pneus", "Inspection", "Révision", "Batterie", "Autre" };

    /// <summary>Matches the website's maintenanceStatus(m): no due date = ok,
    /// overdue = late, within 30 days = soon.</summary>
    public MaintenanceStatus GetStatus()
    {
        if (string.IsNullOrEmpty(NextDueDate) || !DateTime.TryParse(NextDueDate, out var due))
            return MaintenanceStatus.Ok;
        var diffDays = (due.Date - DateTime.Today).Days;
        if (diffDays < 0) return MaintenanceStatus.Late;
        if (diffDays <= 30) return MaintenanceStatus.Soon;
        return MaintenanceStatus.Ok;
    }
}
