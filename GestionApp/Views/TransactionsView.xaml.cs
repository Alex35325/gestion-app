using System.Windows.Controls;

namespace GestionApp.Views;

/// <summary>Reused for both RevenusViewModel and DepensesViewModel via two
/// DataTemplates in App.xaml — one XAML view for all six Revenus/Dépenses
/// screens (Clients/Véhicules/Inventaire), same as the website's
/// renderRevenusTableGeneric/renderDepensesTableGeneric.</summary>
public partial class TransactionsView : UserControl
{
    public TransactionsView()
    {
        InitializeComponent();
    }
}
