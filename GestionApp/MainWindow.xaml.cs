using System.Windows;
using System.Windows.Controls;
using GestionApp.ViewModels;

namespace GestionApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is NavNode node && DataContext is MainViewModel vm)
            vm.SelectNavCommand.Execute(node);
    }
}
