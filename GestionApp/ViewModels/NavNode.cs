using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

/// <summary>
/// One entry in the left nav tree. A node with children acts as both a
/// group header (expand/collapse) and — if CreateViewModel is set — the
/// group's own default view (e.g. clicking "Véhicules" itself shows the
/// vehicle list, while its children show Revenus/Dépenses scoped to
/// vehicles). Mirrors the website's parent/child NAV_ITEMS structure.
/// </summary>
public class NavNode : ViewModelBase
{
    /// <summary>Stable key used for saving/loading per-device nav preferences
    /// (visibility + order) — mirrors the website's NAV_ITEMS ids.</summary>
    public string Id { get; }
    public string Title { get; }
    public Func<object>? CreateViewModel { get; }
    public ObservableCollection<NavNode> Children { get; } = new();

    /// <summary>Colored dot shown next to top-level items — purely cosmetic,
    /// null for children.</summary>
    public Brush? AccentBrush { get; set; }

    private bool _isExpanded = true;
    public bool IsExpanded { get => _isExpanded; set => SetField(ref _isExpanded, value); }

    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }

    private bool _isVisible = true;
    public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value); }

    public NavNode(string id, string title, Func<object>? createViewModel = null)
    {
        Id = id;
        Title = title;
        CreateViewModel = createViewModel;
    }
}
