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
    private readonly Polyline _polyline;
    private readonly InputHandler _inputHandler;
    private readonly PropertyPanelSync _propertyPanelSync;
    private readonly DrawingPersistence _persistence;

    public MainWindow()
    {
        InitializeComponent();

        _renderer = new VisualRenderer(
            DrawingCanvas,            
            _drawing
            );
        _polyline = new Polyline(
            _drawing, 
            DrawingCanvas, 
            _renderer,
            GetCurrentColor,
            GetCurrentThickness
            );

        _persistence = new DrawingPersistence(
            _polyline,
            _renderer
            );

        _inputHandler = new InputHandler(
            _polyline,
            _persistence,
            _renderer,
            _drawing
            );

        DrawingCanvas.Focus();

        _propertyPanelSync = new PropertyPanelSync(_polyline, CbColor, SliderThickness);

    }

    // Вспомогательные методы
    private string GetCurrentColor() => (CbColor.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Black";

    private double GetCurrentThickness() => SliderThickness.Value;

    // Маршрутизация событий
    private void BtnNewPolyline_Click(object sender, RoutedEventArgs e)
    {
        _polyline.StartNewPolyline();        
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        _polyline.DeleteSelected();
    }

    private void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
    {
        _polyline.DeleteAll();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _persistence.SaveToFile(_drawing);
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        _persistence.LoadFromFile();        
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
        var pt = e.GetPosition(DrawingCanvas);
        _inputHandler.HandleMouseUp(pt);
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