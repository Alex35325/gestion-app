namespace GestionApp.Models;

/// <summary>
/// Every row synced with Supabase implements this so Data/AppDataStore can
/// offer one generic Insert/Update/Delete instead of a copy per entity.
/// </summary>
public interface IEntity
{
    string Id { get; }
}
