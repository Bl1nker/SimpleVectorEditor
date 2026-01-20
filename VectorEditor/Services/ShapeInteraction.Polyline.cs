using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Models;

namespace VectorEditor.Services;

public partial class ShapeInteraction
{
    public List<Point> currentPoints = new();
    private Polyline? _currentNewPolyline;
    public bool IsDrawingNewPolyline { get; private set; }


    public void StartNewPolyline()
    {
        CancelNewPolyline();
        IsDrawingNewPolyline = true;
        Mouse.OverrideCursor = Cursors.Cross;
    }

    public void CancelNewPolyline()
    {
        IsDrawingNewPolyline = false;
        currentPoints.Clear();
        ClearSelection();

        if (_currentNewPolyline != null)
        {
            _canvas.Children.Remove(_currentNewPolyline);
            _currentNewPolyline = null;
        }
    }

    public void LineMove(Point pt)
    {
        if (_drawing.Elements[draggedShapeIndex] is PolylineModel pline && _renderer.UIShapes[draggedShapeIndex] is Polyline uiPline)
        {
            var newRefPoint = dragShapeOffset.HasValue ? pt - dragShapeOffset.Value : pt;

            var oldRefPoint = pline.Points[0];

            var delta = newRefPoint - oldRefPoint;

            for (int i = 0; i < pline.Points.Count; i++)
            {
                pline.Points[i] = new Point(pline.Points[i].X + delta.X, pline.Points[i].Y + delta.Y);
                uiPline.Points[i] = pline.Points[i];
            }

            if (SelectedModel == pline)
            {
                for (int i = 0; i < _renderer.Nodes.Count && i < pline.Points.Count; i++)
                {
                    var handle = _renderer.Nodes[i];
                    var p = pline.Points[i];
                    Canvas.SetLeft(handle, p.X - 4);
                    Canvas.SetTop(handle, p.Y - 4);
                }
            }

        }
    }

    public void AddPointToPolyline(Point pt)
    {
        currentPoints.Add(pt);
        UpdateNewPolylinePreview();
    }

    public void TempPolyline(Point pt)
    {
        var temp = new List<Point>(currentPoints) { pt };
        if (_currentNewPolyline == null)
        {
            _currentNewPolyline = new Polyline
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };
            _canvas.Children.Add(_currentNewPolyline);
        }
        _currentNewPolyline.Points.Clear();
        foreach (var p in temp)
        {
            _currentNewPolyline.Points.Add(p);
        }
    }

    public void UpdateNewPolylinePreview()
    {
        if (_currentNewPolyline == null)
        {
            _currentNewPolyline = new Polyline
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };

            _canvas.Children.Add(_currentNewPolyline);
        }

        _currentNewPolyline.Points.Clear();

        foreach (var p in currentPoints)
            _currentNewPolyline.Points.Add(p);
    }

    public bool TryFinishNewPolyline()
    {
        if (!IsDrawingNewPolyline) return false;

        FinishNewPolyline();
        Mouse.OverrideCursor = null;
        _canvas.Focus();

        return true;
    }

    public void FinishNewPolyline()
    {
        if (currentPoints.Count >= 2)
        {
            string color = _getCurrentColor();
            double thickness = _getCurrentThickness();
            var line = new PolylineModel
            {
                Points = new List<Point>(currentPoints),
                Color = color,
                Thickness = thickness
            };
            _drawing.Elements.Add(line);

        }

        CancelNewPolyline();
        _renderer.RefreshDrawing(_drawing, SelectedModel);
    }
}
