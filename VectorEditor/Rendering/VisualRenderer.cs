using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Models;

namespace VectorEditor.Rendering;

public class VisualRenderer
{
    private readonly Canvas _canvas;
    private readonly DrawingModel _drawing;
    public List<Shape> UIShapes { get; set; } = new();

    public List<Ellipse> Nodes { get; set; } = new();

    public VisualRenderer(Canvas canvas, DrawingModel drawing)
    {
        _canvas = canvas;
        _drawing = drawing;
    }

    public void ClearAll()
    {
        _canvas.Children.Clear();
        UIShapes.Clear();
        Nodes.Clear();
    }

    public void RefreshDrawing(DrawingModel drawingModel, VectorEditor.Models.ShapeModel? selectedModel)
    {
        Redraw(drawingModel);
        if (selectedModel != null)
        {
            ShowHandlesFor(selectedModel);
        }
    }

    public void Redraw(DrawingModel model)
    {
        ClearAll();

        foreach (var elem in model.Elements)
        {
            if (elem is PolylineModel pline)
            {
                RedrawPolyline(pline);
                continue;
            }

            // Другие формы
        }
    }

    public void ShowHandlesFor(ShapeModel model)
    {
        ClearHandles();

        if (model is PolylineModel pline)
        {
            foreach (var p in pline.Points)
            {
                var node = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.White,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(node, p.X - 4);
                Canvas.SetTop(node, p.Y - 4);
                _canvas.Children.Add(node);
                Nodes.Add(node);
            }
        }
    }

    public void ClearHandles()
    {
        foreach (var h in Nodes)
        {
            _canvas.Children.Remove(h);
        }
        Nodes.Clear();
    }

    public Shape? GetElementAt(Point pt, double tolerance = 10.0)
    {
        foreach (var obj in UIShapes)
        {
            if (Helpers.Geometry.IsPointOnShape(obj, pt, tolerance))
            {
                return obj;
            }
        }
        return null;
    }

    public Ellipse? GetNodeAt(Point pt)
    {
        foreach (var h in Nodes)
        {
            var left = Canvas.GetLeft(h);
            var top = Canvas.GetTop(h);
            var rect = new Rect(left, top, h.Width, h.Height);
            if (rect.Contains(pt))
                return h;
        }
        return null;
    }

    public int GetNodeIndex(Ellipse handle) => Nodes.IndexOf(handle);

    public void SetDrawingModel(DrawingModel? model)
    {
        if (model == null)
            return;

        _drawing.Elements.Clear();

        foreach (var obj in model.Elements)
        {
            if (obj is PolylineModel pline)
            {
                _drawing.Elements.Add(new PolylineModel
                {
                    Points = new List<Point>(pline.Points),
                    Color = pline.Color,
                    Thickness = pline.Thickness,
                });
            }
        }

        Redraw(_drawing);
    }

    private void RedrawPolyline(PolylineModel obj)
    {
        var line = new Polyline
        {
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(obj.Color)),
            StrokeThickness = obj.Thickness,
            Fill = Brushes.Transparent
        };

        foreach (var p in obj.Points)
        {
            line.Points.Add(p);
        }

        _canvas.Children.Add(line);
        UIShapes.Add(line);
    }
}
