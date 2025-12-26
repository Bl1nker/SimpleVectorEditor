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
    public List<Polyline> UILines { get; set; } = new();
    public List<Ellipse> Nodes { get; set; } = new();

    public VisualRenderer(Canvas canvas, DrawingModel drawing)
    {
        _canvas = canvas;        
        _drawing = drawing;
    }

    public void ClearAll()
    {
        _canvas.Children.Clear();
        UILines.Clear();
        Nodes.Clear();
    }

    public void RefreshDrawing(DrawingModel drawingModel, PolylineModel? selectedModel)
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

        foreach (var m in model.Polylines)
        {
            var line = new Polyline
            {
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(m.Color)),
                StrokeThickness = m.Thickness,
                Fill = Brushes.Transparent
            };

            foreach (var p in m.Points)
            {
                line.Points.Add(p);
            }

            _canvas.Children.Add(line);
            UILines.Add(line);
        }
    }

    public void ShowHandlesFor(PolylineModel model)
    {
        ClearHandles();

        foreach (var p in model.Points)
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

    public void ClearHandles()
    {
        foreach (var h in Nodes)
        {
            _canvas.Children.Remove(h);
        }
        Nodes.Clear();
    }

    public Polyline? GetLineAt(Point pt, double tolerance = 10.0)
    {
        foreach (var pl in UILines)
        {
            if (Helpers.Geometry.IsPointOnPolyline(pl.Points, pt, tolerance))
            {
                return pl;
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

    public void SetDrawingModel(DrawingModel model)
    {
        if (model == null) 
            return;

        _drawing.Polylines.Clear();

        foreach (var line in model.Polylines)
        {
            _drawing.Polylines.Add(new PolylineModel
            {
                Points = new List<Point>(line.Points),
                Color = line.Color,
                Thickness = line.Thickness,
            });
        }

        Redraw(_drawing);
    }
}
