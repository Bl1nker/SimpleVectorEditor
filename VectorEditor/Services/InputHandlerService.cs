using System.Windows.Input;
using System.Windows.Controls;
using VectorEditor.Rendering;

namespace VectorEditor.Services;

public class InputHandlerService
{
    private readonly PolylineInteractionService _interactionService;
    private readonly DrawingPersistenceService _persistenceService;
    private readonly VisualRenderer _renderer;
    private readonly Canvas _canvas;    
    private readonly Func<string> _getCurrentColor;
    private readonly Func<double> _getCurrentThickness;

    public InputHandlerService(
        PolylineInteractionService interactionService,
        DrawingPersistenceService persistenceService,
        VisualRenderer renderer,
        Canvas canvas,        
        Func<string> getCurrentColor,
        Func<double> getCurrentThickness
        )
    {
        _interactionService = interactionService;
        _persistenceService = persistenceService;
        _renderer = renderer;
        _canvas = canvas;        
        _getCurrentColor = getCurrentColor;
        _getCurrentThickness = getCurrentThickness;
    }

    // Завершение рисования 
    public void HandlePreviewKeyDown(KeyEventArgs e)
    {
        if (_interactionService.IsDrawingNew)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                FinishNewPolyline();
                e.Handled = true;
                return;
            }
        }

        if (e.Key == Key.Delete)
        {
            _interactionService.DeleteSelected();
            e.Handled = true;
            return;
        }
    }

    // Горячие клавиши для Save/Open
    public void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (_interactionService.IsDrawingNew)
            {
                FinishNewPolyline();
            }

            _persistenceService.SaveToFile(_interactionService.DrawingModel);

            e.Handled = true;
        }
        else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
        {
            var model = _persistenceService.LoadFromFile();
            _interactionService.SetDrawingModel(model);
            _renderer.Redraw(model);
            _interactionService.ClearSelection();

            e.Handled = true;
        }
    }

    private void FinishNewPolyline()
    {
        var color = _getCurrentColor();
        var thickness = _getCurrentThickness();
        _interactionService.TryFinishNewPolyline(color, thickness);
        Mouse.OverrideCursor = null;
        _canvas.Focus();
    }
}
