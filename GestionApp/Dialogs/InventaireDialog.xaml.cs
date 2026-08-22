using System.Globalization;
using System.Windows;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.Dialogs;

public partial class InventaireDialog : Window
{
    private readonly InventaireItem? _editing;

    public InventaireItem Result { get; private set; } = new();

    public InventaireDialog(InventaireItem? editing = null)
    {
        InitializeComponent();
        _editing = editing;

        if (editing != null)
        {
            Title = "Modifier l'article";
            NameBox.Text = editing.Name;
            QuantityBox.Text = editing.Quantity.ToString(CultureInfo.InvariantCulture);
            UnitBox.Text = editing.Unit;
            CategoryBox.Text = editing.Category;
            MinThresholdBox.Text = editing.MinThreshold?.ToString(CultureInfo.InvariantCulture) ?? "";
            NotesBox.Text = editing.Notes;
        }
        else
        {
            Title = "Ajouter un article";
            QuantityBox.Text = "0";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Le nom est obligatoire.", "Champ manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!TryParseDecimal(QuantityBox.Text, out var quantity))
        {
            MessageBox.Show(this, "La quantité doit être un nombre.", "Champ invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        decimal? minThreshold = null;
        if (!string.IsNullOrWhiteSpace(MinThresholdBox.Text))
        {
            if (!TryParseDecimal(MinThresholdBox.Text, out var mt))
            {
                MessageBox.Show(this, "Le seuil d'alerte doit être un nombre.", "Champ invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            minThreshold = mt;
        }

        var now = Ids.NowMs();
        Result = new InventaireItem
        {
            Id = _editing?.Id ?? Ids.NewId(),
            Name = name,
            Quantity = quantity,
            Unit = UnitBox.Text.Trim(),
            Category = CategoryBox.Text.Trim(),
            MinThreshold = minThreshold,
            Notes = NotesBox.Text.Trim(),
            CreatedAt = _editing?.CreatedAt ?? now,
            UpdatedAt = now
        };
        DialogResult = true;
    }

    private static bool TryParseDecimal(string text, out decimal value)
        => decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value)
        || decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value);

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
