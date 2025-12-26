using System.Windows;
using System.Windows.Controls;

namespace VectorEditor.Services;

public class PropertyPanelSync
{
    private readonly Polyline _polyline;
    private readonly ComboBox _colorCombobox;
    private readonly Slider _thicknessSlider;

    public PropertyPanelSync(Polyline polyline, ComboBox colorCombobox, Slider thicknessSlider)
    {
        _polyline = polyline;
        _colorCombobox = colorCombobox;
        _thicknessSlider = thicknessSlider;

        _polyline.SelectedLineChanged += SyncPanelWithSelection;

        _colorCombobox.SelectionChanged += OnColorChanged;
        _thicknessSlider.ValueChanged += OnThicknessChanged;
    }

    private void SyncPanelWithSelection()
    {
        bool hasSelection = _polyline.SelectedModel != null;
        _colorCombobox.IsEditable = hasSelection;
        _thicknessSlider.IsEnabled = hasSelection;

        if (hasSelection)
        {
            var (color, thickness) = _polyline.GetSelectedPolylineProperties();

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
        if (_polyline.SelectedModel != null && _colorCombobox.SelectedItem is ComboBoxItem item)
        {
            string color = item.Content.ToString() ?? "Black";
            _polyline.UpdateSelectedPolylineColor(color);

        }
    }

    private void OnThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_polyline.SelectedModel != null)
        {
            _polyline.UpdateSelectedPolylineThickness(e.NewValue);
        }
    }
}
