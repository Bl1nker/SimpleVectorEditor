using System.Windows.Input;
using VectorEditor.Models;
using VectorEditor.Rendering;

namespace VectorEditor.Services;

partial class InputHandler
{
    private readonly Polyline _polyline;
    private readonly DrawingPersistence _persistence;
    private readonly VisualRenderer _renderer;
    private readonly DrawingModel _drawing;

    public InputHandler(
        Polyline polyline,
        DrawingPersistence persistence,
        VisualRenderer renderer,
        DrawingModel drawing
        )
    {
        _polyline = polyline;
        _persistence = persistence;
        _renderer = renderer;
        _drawing = drawing;
    }

    // Завершение рисования 
    public void HandlePreviewKeyDown(KeyEventArgs e)
    {
        if (_polyline.IsDrawingNew)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                _polyline.TryFinishNewPolyline();
                e.Handled = true;
                return;
            }
        }

        if (e.Key == Key.Delete)
        {
            _polyline.DeleteSelected();
            e.Handled = true;
            return;
        }
    }

    // Горячие клавиши для Save/Open
    public void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (_polyline.IsDrawingNew)
            {
                _polyline.TryFinishNewPolyline();
            }

            _persistence.SaveToFile(_polyline.DrawingModel);

            e.Handled = true;
        }
        else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
        {
            _persistence.LoadFromFile();

            e.Handled = true;
        }
    }
}
