using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using VectorEditor.Models;
using VectorEditor.Rendering;
using VectorEditor.Services;

namespace VectorEditor;

public partial class MainWindow : Window
{
    private readonly DrawingModel _drawing = new();
    private readonly VisualRenderer _renderer;
    private readonly ShapeInteraction _shape;
    private readonly InputHandler _inputHandler;
    private readonly PropertyPanelSync _propertyPanelSync;

    public MainWindow()
    {
        InitializeComponent();

        _renderer = new VisualRenderer(
            DrawingCanvas,
            _drawing
            );
        _shape = new ShapeInteraction(
            _drawing,
            DrawingCanvas,
            _renderer,
            GetCurrentColor,
            GetCurrentThickness
            );

        _inputHandler = new InputHandler(
            _shape,
            _renderer,
            _drawing
            );

        DrawingCanvas.Focus();

        _propertyPanelSync = new PropertyPanelSync(_shape, CbColor, SliderThickness);

    }

    // Вспомогательные методы
    private string GetCurrentColor() => (CbColor.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Black";

    private double GetCurrentThickness() => SliderThickness.Value;

    // Маршрутизация событий
    private void BtnNewPolyline_Click(object sender, RoutedEventArgs e)
    {
        _shape.StartNewPolyline();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        _shape.DeleteSelected();
    }

    private void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
    {
        _shape.DeleteAll();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        DrawingPersistence.SaveToFile(_drawing);
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        _shape.ClearSelection();

        var model = DrawingPersistence.LoadFromFile();

        _renderer.SetDrawingModel(model);
    }

    private void DrawingCanvas_MouseLeftBtnDown(object sender, MouseButtonEventArgs e)
    {
        var pt = e.GetPosition(DrawingCanvas);
        _inputHandler.HandleLeftMouseDown(pt);
    }

    private void DrawingCanvas_MouseRightBtnDown(object sender, MouseButtonEventArgs e)
    {
        _inputHandler.HandleMouseRightBtnDown(e);
    }

    private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        var pt = e.GetPosition(DrawingCanvas);
        _inputHandler.HandleMouseMove(pt, e.LeftButton == MouseButtonState.Pressed);
    }

    private void DrawingCanvas_MouseLeftBtnUp(object sender, MouseButtonEventArgs e)
    {        
        _inputHandler.HandleMouseUp();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        _inputHandler.HandlePreviewKeyDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        _inputHandler.HandleKeyDown(e);
        base.OnKeyDown(e);
    }
}