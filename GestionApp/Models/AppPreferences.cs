using System.Collections.Generic;

namespace GestionApp.Models;

/// <summary>
/// Per-device UI preferences (which nav tabs show, in what order, which
/// dashboard widgets show) — saved locally, never synced through Supabase.
/// Same distinction the website makes: this is how *this device* likes to
/// view the shared data, not shared data itself.
/// </summary>
public class AppPreferences
{
    /// <summary>List order = display order for top-level nav items. Ids not
    /// present here keep their default position (forward-compatible with new
    /// sections added later); ids no longer known are ignored.</summary>
    public List<NavItemPref> NavItems { get; set; } = new();
    public List<DashboardWidgetPref> DashboardWidgets { get; set; } = new();
    public bool DashboardShowActivity { get; set; } = true;
}

public class NavItemPref
{
    public string Id { get; set; } = "";
    public bool Visible { get; set; } = true;
}

public class DashboardWidgetPref
{
    public string Id { get; set; } = "";
    public bool Visible { get; set; } = true;
}
