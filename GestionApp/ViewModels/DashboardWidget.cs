using System;
using System.Windows.Media;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

/// <summary>One KPI card on the dashboard. IsVisible is the user's saved
/// preference; DashboardViewModel decides whether to actually show it
/// (always when visible, or dimmed-but-shown while customizing).</summary>
public class DashboardWidget : ViewModelBase
{
    public string Id { get; }
    public string Title { get; }
    public string ValueText { get; }
    public Brush Background { get; }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set { if (SetField(ref _isVisible, value)) VisibilityChanged?.Invoke(); }
    }

    /// <summary>Raised after IsVisible changes from a user action (not the
    /// initial load) so the owner can persist the new preference.</summary>
    public event Action? VisibilityChanged;

    public DashboardWidget(string id, string title, string valueText, Brush background)
    {
        Id = id;
        Title = title;
        ValueText = valueText;
        Background = background;
    }
}
