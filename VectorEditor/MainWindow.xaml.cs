using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Win32;
using VectorEditor.Models;
using VectorEditor.Rendering;
using VectorEditor.Services;


namespace VectorEditor;

public partial class MainWindow : Window
{
    private readonly DrawingModel _drawing = new();
    private readonly VisualRenderer _renderer;
    private readonly PolylineInteractionService _interactionService;
    private readonly InputHandlerService _inputHandler;
    private readonly PropertyPanelSyncService _propertyPanelSync;
    private readonly DrawingPersistenceService _persistenceService;

    public MainWindow()
    {
        InitializeComponent();

        _renderer = new VisualRenderer(DrawingCanvas);
        _interactionService = new PolylineInteractionService(_drawing, DrawingCanvas, _renderer);
        _persistenceService = new DrawingPersistenceService(GetSaveFilePath, GetOpenFilePath);

        _inputHandler = new InputHandlerService(
            _interactionService,
            _persistenceService,
            _renderer,
            DrawingCanvas,
            GetCurrentColor,
            GetCurrentThickness
            );

        DrawingCanvas.Focus();

        _propertyPanelSync = new PropertyPanelSyncService(_interactionService, CbColor, SliderThickness);

    }

    // Вспомогательные методы
    private string GetCurrentColor() => (CbColor.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Black";

    private double GetCurrentThickness() => SliderThickness.Value;

    private string? GetSaveFilePath()
    {
        var dlg = new SaveFileDialog { Filter = "JSON files (*.json)|*.json" };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    private string? GetOpenFilePath()
    {
        var dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    // Маршрутизация событий
    private void BtnNewPolyline_Click(object sender, RoutedEventArgs e)
    {
        _interactionService.StartNewPolyline();
        Mouse.OverrideCursor = Cursors.Cross;
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        _interactionService.DeleteSelected();
    }

    private void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
    {
        _interactionService.DeleteAll();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _persistenceService.SaveToFile(_drawing);
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        var model = _persistenceService.LoadFromFile();
        _interactionService.SetDrawingModel(model);
        _renderer.Redraw(model);
        _interactionService.ClearSelection();
    }

    private void DrawingCanvas_MouseLeftBtnDown(object sender, MouseButtonEventArgs e)
    {
        var pt = e.GetPosition(DrawingCanvas);
        _interactionService.HandleLeftMouseDown(pt);
    }

    private void DrawingCanvas_MouseRightBtnDown(object sender, MouseButtonEventArgs e)
    {
        _interactionService.HandleMouseRightBtnDown(GetCurrentColor(), GetCurrentThickness(), e);
    }

    private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        var pt = e.GetPosition(DrawingCanvas);
        _interactionService.HandleMouseMove(pt, e.LeftButton == MouseButtonState.Pressed);
    }

    private void DrawingCanvas_MouseLeftBtnUp(object sender, MouseButtonEventArgs e)
    {
        var pt = e.GetPosition(DrawingCanvas);
        _interactionService.HandleMouseUp(pt);
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