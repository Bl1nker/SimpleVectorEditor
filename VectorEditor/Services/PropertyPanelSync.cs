using System.Windows;
using System.Windows.Controls;

namespace VectorEditor.Services;

public class PropertyPanelSync
{
    private readonly ShapeInteraction _shape;
    private readonly ComboBox _colorCombobox;
    private readonly Slider _thicknessSlider;

    public PropertyPanelSync(ShapeInteraction shape, ComboBox colorCombobox, Slider thicknessSlider)
    {
        _shape = shape;
        _colorCombobox = colorCombobox;
        _thicknessSlider = thicknessSlider;

        _shape.SelectedShapeChanged += SyncPanelWithSelection;

        _colorCombobox.SelectionChanged += OnColorChanged;
        _thicknessSlider.ValueChanged += OnThicknessChanged;
    }

    private void SyncPanelWithSelection()
    {
        bool hasSelection = _shape.SelectedModel != null;
        _colorCombobox.IsEditable = hasSelection;
        _thicknessSlider.IsEnabled = hasSelection;

        if (hasSelection)
        {
            var (color, thickness) = _shape.GetSelectedShapeProperties();

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
        if (_shape.SelectedModel != null && _colorCombobox.SelectedItem is ComboBoxItem item)
        {
            string color = item.Content.ToString() ?? "Black";
            _shape.UpdateSelectedShapeColor(color);

        }
    }

    private void OnThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_shape.SelectedModel != null)
        {
            _shape.UpdateSelectedShapeThickness(e.NewValue);
        }
    }
}
