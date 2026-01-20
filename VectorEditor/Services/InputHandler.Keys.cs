using System.Windows.Input;
using VectorEditor.Models;
using VectorEditor.Rendering;

namespace VectorEditor.Services;

partial class InputHandler
{
    private readonly ShapeInteraction _shape;
    private readonly VisualRenderer _renderer;
    private readonly DrawingModel _drawing;

    public InputHandler(
        ShapeInteraction shape,
        VisualRenderer renderer,
        DrawingModel drawing
        )
    {
        _shape = shape;
        _renderer = renderer;
        _drawing = drawing;
    }

    // Завершение рисования 
    public void HandlePreviewKeyDown(KeyEventArgs e)
    {
        if (_shape.IsDrawingNewPolyline)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                _shape.TryFinishNewPolyline();
                e.Handled = true;
                return;
            }
        }

        if (e.Key == Key.Delete)
        {
            _shape.DeleteSelected();
            e.Handled = true;
            return;
        }
    }

    // Горячие клавиши для Save/Open
    public void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (_shape.IsDrawingNewPolyline)
            {
                _shape.TryFinishNewPolyline();
            }

            DrawingPersistence.SaveToFile(_shape.DrawingModel);

            e.Handled = true;
        }
        else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
        {
            _shape.ClearSelection();

            var model = DrawingPersistence.LoadFromFile();

            _renderer.SetDrawingModel(model);

            e.Handled = true;
        }
    }
}
