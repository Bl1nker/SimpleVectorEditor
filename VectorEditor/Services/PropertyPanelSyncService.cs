using System.Windows;
using System.Windows.Controls;

namespace VectorEditor.Services;

public class PropertyPanelSyncService
{
    private readonly PolylineInteractionService _interactionService;
    private readonly ComboBox _colorCombobox;
    private readonly Slider _thicknessSlider;

    public PropertyPanelSyncService(PolylineInteractionService interactionService, ComboBox colorCombobox, Slider thicknessSlider)
    {
        _interactionService = interactionService;
        _colorCombobox = colorCombobox;
        _thicknessSlider = thicknessSlider;

        _interactionService.SelectedLineChanged += SyncPanelWithSelection;

        _colorCombobox.SelectionChanged += OnColorChanged;
        _thicknessSlider.ValueChanged += OnThicknessChanged;
    }

    private void SyncPanelWithSelection()
    {
        bool hasSelection = _interactionService.SelectedModel != null;
        _colorCombobox.IsEditable = hasSelection;
        _thicknessSlider.IsEnabled = hasSelection;

        if (hasSelection)
        {
            var (color, thickness) = _interactionService.GetSelectedLineProperties();

            var item = _colorCombobox.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == color);

            if (item != null)
            {
                _colorCombobox.SelectedItem = item;
            }
            else _colorCombobox.SelectedIndex = 0;

            _thicknessSlider.Value = thickness;
        }
        else
        {
            _colorCombobox.SelectedIndex = 0;
            _thicknessSlider.Value = 2;
        }
    }

    private void OnColorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_interactionService.SelectedModel != null && _colorCombobox.SelectedItem is ComboBoxItem item)
        {
            string color = item.Content.ToString() ?? "Black";
            _interactionService.UpdateSelectedLineColor(color);

        }
    }

    private void OnThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_interactionService.SelectedModel != null)
        {
            _interactionService.UpdateSelectedLineThickness(e.NewValue);
        }
    }
}
