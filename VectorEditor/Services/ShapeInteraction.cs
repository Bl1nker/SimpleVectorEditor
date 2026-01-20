using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Models;
using VectorEditor.Rendering;

namespace VectorEditor.Services;

public partial class ShapeInteraction
{
    private readonly DrawingModel _drawing;
    private readonly Canvas _canvas;
    private readonly VisualRenderer _renderer;
    public ShapeModel? SelectedModel { get; private set; }
    
    
    // Перетаскивание фигуры
    public int draggedShapeIndex = -1;
    public Vector? dragShapeOffset;
    
    // Перетаскивание узла фигуры
    public int draggedShapeNodeIndex = -1;
    public int draggedPointIndex = -1;
    public Vector? dragNodeOffset;
    
    public DrawingModel DrawingModel => _drawing;
    public event Action? SelectedShapeChanged;

    private readonly Func<string> _getCurrentColor;
    private readonly Func<double> _getCurrentThickness;

    public ShapeInteraction(DrawingModel drawing, Canvas canvas, VisualRenderer renderer, Func<string> getCurrentColor, Func<double> getCurrentThickness)
    {
        _drawing = drawing;
        _canvas = canvas;
        _renderer = renderer;
        _getCurrentColor = getCurrentColor;
        _getCurrentThickness = getCurrentThickness;
    }    


    // Общие методы
    public void DeleteSelected()
    {
        if (SelectedModel == null) return;

        _drawing.Elements.Remove(SelectedModel);
        SelectedModel = null;
        _renderer.RefreshDrawing(_drawing, SelectedModel);
        _renderer.ClearHandles();
    }

    public void DeleteAll()
    {
        if (_drawing.Elements.Count == 0) return;
        _drawing.Elements.Clear();
        SelectedModel = null;
        _renderer.RefreshDrawing(_drawing, SelectedModel);
        _renderer.ClearHandles();
    }

    public void SelectObject(Shape obj)
    {
        var index = _renderer.UIShapes.IndexOf(obj);
        if (index >= 0 && index < _drawing.Elements.Count)
        {
            SelectedModel = _drawing.Elements[index];
            _renderer.ShowHandlesFor(SelectedModel);
            SelectedShapeChanged?.Invoke();
        }
    }

    public void ClearSelection()
    {
        SelectedModel = null;
        _renderer.ClearHandles();
    }

    public void UpdateSelectedShapeColor(string color)
    {
        if (SelectedModel != null)
        {
            SelectedModel.Color = color;
            _renderer.RefreshDrawing(_drawing, SelectedModel);
        }
    }

    public void UpdateSelectedShapeThickness(double thickness)
    {
        if (SelectedModel != null)
        {
            SelectedModel.Thickness = thickness;
            _renderer.RefreshDrawing(_drawing, SelectedModel);
        }
    }

    public (string color, double thickness) GetSelectedShapeProperties()
    {
        if (SelectedModel != null)
        {
            return (SelectedModel.Color, SelectedModel.Thickness);
        }
        return ("Black", 2.0);
    }


    public void NodeMove(Point pt)
    {
        var model = _drawing.Elements[draggedShapeNodeIndex];
        

        if (model is PolylineModel pline && draggedPointIndex < pline.Points.Count)
        {

            var newPosition = dragNodeOffset.HasValue ? pt - dragNodeOffset.Value : pt;

            pline.Points[draggedPointIndex] = newPosition;

            if (draggedShapeNodeIndex < _renderer.UIShapes.Count)
            {
                var uiObject = _renderer.UIShapes[draggedShapeNodeIndex];

                if (uiObject is Polyline uiPline && draggedPointIndex < uiPline.Points.Count)
                {
                    uiPline.Points[draggedPointIndex] = newPosition;
                }
            }

            if (SelectedModel == model && draggedPointIndex < _renderer.Nodes.Count)
            {
                var handle = _renderer.Nodes[draggedPointIndex];
                Canvas.SetLeft(handle, newPosition.X - 4);
                Canvas.SetTop(handle, newPosition.Y - 4);
            }
        }
    }
        
    public void AddNewNode(Point pt)
    {
        if (SelectedModel != null && SelectedModel is PolylineModel pline)
        {
            var segment = Helpers.Geometry.FindNearestSegmentInsertion(new PointCollection(pline.Points), pt);
            if (segment.HasValue)
            {
                pline.Points.Insert(segment.Value.segmentIndex, segment.Value.projection);
                _renderer.RefreshDrawing(_drawing, SelectedModel);
                _renderer.ShowHandlesFor(SelectedModel);
            }
        }
    }
}
