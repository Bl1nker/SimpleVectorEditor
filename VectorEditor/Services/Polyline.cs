using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Models;
using VectorEditor.Rendering;

namespace VectorEditor.Services;

public class Polyline
{
    private readonly DrawingModel _drawing;
    private readonly Canvas _canvas;
    private readonly VisualRenderer _renderer;

    public PolylineModel? SelectedModel { get; private set; }
    public bool IsDrawingNew { get; private set; }

    public List<Point> currentPoints = new();
    private System.Windows.Shapes.Polyline? _currentNewPolyline;

    public int draggedLineNodeIndex = -1;
    public int draggedPointIndex = -1;
    public Vector? dragNodeOffset;

    public int draggedPolylineIndex = -1;
    public Vector? dragPolylineOffset;

    public DrawingModel DrawingModel => _drawing;

    public event Action? SelectedLineChanged;

    private readonly Func<string> _getCurrentColor;
    private readonly Func<double> _getCurrentThickness;

    public Polyline(DrawingModel drawing, Canvas canvas, VisualRenderer renderer, Func<string> getCurrentColor, Func<double> getCurrentThickness)
    {
        _drawing = drawing;
        _canvas = canvas;
        _renderer = renderer;
        _getCurrentColor = getCurrentColor;
        _getCurrentThickness = getCurrentThickness;
    }

    public void StartNewPolyline()
    {
        CancelNewPolyline();
        IsDrawingNew = true;
        Mouse.OverrideCursor = Cursors.Cross;
    }
    
    public void DeleteSelected()
    {
        if (SelectedModel == null) return;

        _drawing.Polylines.Remove(SelectedModel);
        SelectedModel = null;
        _renderer.RefreshDrawing(_drawing, SelectedModel);
        _renderer.ClearHandles();
    }

    public void DeleteAll()
    {
        if (_drawing.Polylines.Count == 0) return;
        _drawing.Polylines.Clear();
        SelectedModel = null;
        _renderer.RefreshDrawing(_drawing, SelectedModel);
        _renderer.ClearHandles();
    }

    public void SelectPolyline(System.Windows.Shapes.Polyline pl)
    {
        var index = _renderer.UILines.IndexOf(pl);
        if (index >= 0 && index < _drawing.Polylines.Count)
        {
            SelectedModel = _drawing.Polylines[index];
            _renderer.ShowHandlesFor(SelectedModel);
            SelectedLineChanged?.Invoke();
        }
    }

    public void NodeMove(Point pt, int nodeIdx, int pointIdx, Vector? offset)
    {
        var model = _drawing.Polylines[nodeIdx];
        if (pointIdx < model.Points.Count)
        {

            var newPosition = offset.HasValue ? pt - offset.Value : pt;

            model.Points[pointIdx] = newPosition;

            if (nodeIdx < _renderer.UILines.Count)
            {
                var uiPolyline = _renderer.UILines[nodeIdx];
                if (pointIdx < uiPolyline.Points.Count)
                {
                    uiPolyline.Points[pointIdx] = newPosition;
                }
            }

            if (SelectedModel == model && pointIdx < _renderer.Nodes.Count)
            {
                var handle = _renderer.Nodes[pointIdx];
                Canvas.SetLeft(handle, newPosition.X - 4);
                Canvas.SetTop(handle, newPosition.Y - 4);
            }
        }
    }

    public void LineMove(Point pt, int lineIdx, Vector? offset)
    {
        var model = _drawing.Polylines[lineIdx];
        var uiPolyline = _renderer.UILines[lineIdx];

        var newReferencePoint = offset.HasValue ? pt - offset.Value : pt;

        var oldReferencePoint = model.Points[0];

        var delta = newReferencePoint - oldReferencePoint;

        for (int i = 0; i < model.Points.Count; i++)
        {
            model.Points[i] = new Point(model.Points[i].X + delta.X, model.Points[i].Y + delta.Y);
            uiPolyline.Points[i] = model.Points[i];
        }

        if (SelectedModel == model)
        {
            for (int i = 0; i < _renderer.Nodes.Count && i < model.Points.Count; i++)
            {
                var handle = _renderer.Nodes[i];
                var p = model.Points[i];
                Canvas.SetLeft(handle, p.X - 4);
                Canvas.SetTop(handle, p.Y - 4);
            }
        }
    }

    public void AddPointToPolyline(Point pt)
    {
        currentPoints.Add(pt);
        UpdateNewPolylinePreview();
    }

    public void AddNewNode(int idx, Point pt)
    {
        if (SelectedModel != null)
        {
            SelectedModel.Points.Insert(idx, pt);
            _renderer.RefreshDrawing(_drawing, SelectedModel);
            _renderer.ShowHandlesFor(SelectedModel);
        }
    }

    public void TempPolyline(Point pt)
    {
        var temp = new List<Point>(currentPoints) { pt };
        if (_currentNewPolyline == null)
        {
            _currentNewPolyline = new System.Windows.Shapes.Polyline
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

    public void ClearSelection()
    {
        SelectedModel = null;
        _renderer.ClearHandles();
    }

    public void UpdateNewPolylinePreview()
    {
        if (_currentNewPolyline == null)
        {
            _currentNewPolyline = new System.Windows.Shapes.Polyline
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
        if (!IsDrawingNew) return false;

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
            _drawing.Polylines.Add(line);

        }

        CancelNewPolyline();
        _renderer.RefreshDrawing(_drawing, SelectedModel);
    }

    public void CancelNewPolyline()
    {
        IsDrawingNew = false;
        currentPoints.Clear();
        ClearSelection();

        if (_currentNewPolyline != null)
        {
            _canvas.Children.Remove(_currentNewPolyline);
            _currentNewPolyline = null;
        }
    }

    public void UpdateSelectedPolylineColor(string color)
    {
        if (SelectedModel != null)
        {
            SelectedModel.Color = color;
            _renderer.RefreshDrawing(_drawing, SelectedModel);
        }
    }

    public void UpdateSelectedPolylineThickness(double thickness)
    {
        if (SelectedModel != null)
        {
            SelectedModel.Thickness = thickness;
            _renderer.RefreshDrawing(_drawing, SelectedModel);
        }
    }

    public (string color, double thickness) GetSelectedPolylineProperties()
    {
        if (SelectedModel != null)
        {
            return (SelectedModel.Color, SelectedModel.Thickness);
        }
        return ("Black", 2.0);
    }
}
