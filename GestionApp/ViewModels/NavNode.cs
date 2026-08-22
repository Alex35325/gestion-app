using System;
using System.Collections.ObjectModel;
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
    public string Title { get; }
    public Func<object>? CreateViewModel { get; }
    public ObservableCollection<NavNode> Children { get; } = new();

    private bool _isExpanded = true;
    public bool IsExpanded { get => _isExpanded; set => SetField(ref _isExpanded, value); }

    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }

    public NavNode(string title, Func<object>? createViewModel = null)
    {
        Title = title;
        CreateViewModel = createViewModel;
    }
}
