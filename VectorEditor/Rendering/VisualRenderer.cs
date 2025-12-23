using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Models;
using VectorEditor.Helpers;

namespace VectorEditor.Rendering;

public class VisualRenderer
{
    private readonly Canvas _canvas;
    public List<Polyline> UIPolylines { get; set; } = new();
    public List<Ellipse> Handles { get; set; } = new();

    public VisualRenderer(Canvas canvas)
    {
        _canvas = canvas;
    }

    public void ClearAll()
    {
        _canvas.Children.Clear();
        UIPolylines.Clear();
        Handles.Clear();
    }

    public void Redraw(DrawingModel model)
    {
        ClearAll();

        foreach (var m in model.Polylines)
        {
            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(m.Color)),
                StrokeThickness = m.Thickness,
                Fill = Brushes.Transparent
            };

            foreach (var p in m.Points)
            {
                polyline.Points.Add(p);
            }

            _canvas.Children.Add(polyline);
            UIPolylines.Add(polyline);
        }
    }

    public void ShowHandlesFor(PolylineModel model)
    {
        ClearHandles();

        foreach (var p in model.Points)
        {
            var handle = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.White,
                Stroke = Brushes.Blue,
                StrokeThickness = 1
            };

            Canvas.SetLeft(handle, p.X - 4);
            Canvas.SetTop(handle, p.Y - 4);
            _canvas.Children.Add(handle);
            Handles.Add(handle);
        }
    }

    public void ClearHandles()
    {
        foreach (var h in Handles)
        {
            _canvas.Children.Remove(h);
        }
        Handles.Clear();
    }

    public Polyline? GetPolylineAt(Point pt, double tolerance = 10.0)
    {
        foreach (var pl in UIPolylines)
        {
            if (GeometryHelper.IsPointOnPolyline(pl.Points, pt, tolerance))
            {
                return pl;
            }
        }
        return null;
    }

    public Ellipse? GetHandleAt(Point pt)
    {
        foreach (var h in Handles)
        {
            var left = Canvas.GetLeft(h);
            var top = Canvas.GetTop(h);
            var rect = new Rect(left, top, h.Width, h.Height);
            if (rect.Contains(pt)) 
                return h;
        }
        return null;
    }

    public int GetHandleIndex(Ellipse handle) => Handles.IndexOf(handle);
}
