using System.Windows;
using System.Windows.Controls;
using GestionApp.ViewModels;

namespace GestionApp.Views;

public partial class VehiculesInfoView : UserControl
{
    public VehiculesInfoView()
    {
        InitializeComponent();
    }

    private void Card_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is VehiculeCard card && DataContext is VehiculesInfoViewModel vm)
            vm.SelectedCard = card;
    }
}
