using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using VectorEditor.Models;
using VectorEditor.Rendering;
using VectorEditor.Helpers;

namespace VectorEditor.Services;

public class PolylineInteractionService
{
    private readonly DrawingModel _drawing;
    private readonly Canvas _canvas;
    private readonly VisualRenderer _renderer;

    public PolylineModel? SelectedModel { get; private set; }
    public bool IsDrawingNew { get; private set; }

    private List<Point> _currentPoints = new();
    private Polyline? _currentNewPolyline;

    private int _draggedPolylineNodeIndex = -1;
    private int _draggedPointIndex = -1;
    private Vector? _dragOffsetForNode;

    private int _draggedPolylineIndex = -1;
    private Vector? _dragOffsetForPolyline;

    // Для определения двойного щелчка
    private DateTime _lastClickTime = DateTime.MinValue;
    private Point _lastClickPosition;
    private const double MaxDoubleClickTime = 400; // ms
    private const double MaxDoubleClickDistance = 10; // px
    
    public DrawingModel DrawingModel => _drawing;

    public event Action? SelectedLineChanged;

    

    public PolylineInteractionService(DrawingModel drawing, Canvas canvas, VisualRenderer renderer)
    {
        _drawing = drawing;
        _canvas = canvas;
        _renderer = renderer;
    }

    public void StartNewPolyline()
    {
        CancelNewPolyline();
        IsDrawingNew = true;
    }
    
    public void SetDrawingModel(DrawingModel model)
    {
        if (model == null) return;

        _drawing.Polylines.Clear();

        foreach (var polyline in model.Polylines)
        {
            _drawing.Polylines.Add(new PolylineModel
            {
                Points = new List<Point>(polyline.Points),
                Color = polyline.Color,
                Thickness = polyline.Thickness,
            });
        }
        ClearSelection();
        RefreshDrawing();
    }
    
    public void HandleLeftMouseDown(Point pt)
    {
        var now = DateTime.Now;
        var timeDiff = (now - _lastClickTime).TotalMilliseconds;
        var dist = (pt - _lastClickPosition).Length;

        bool isDoubleClick = timeDiff < MaxDoubleClickTime && dist < MaxDoubleClickDistance;

        _lastClickTime = now;
        _lastClickPosition = pt;

        // Обработка двойного клика
        if (isDoubleClick)
        {
            HandleDoubleClickInternal(pt);
            return;
        }

        // Обработка одинарного клика
        if (_draggedPolylineNodeIndex >= 0)
            return;

        //Рисуем новую линию
        if (IsDrawingNew)
        {
            _currentPoints.Add(pt);
            UpdateNewPolylinePreview();
            return;
        }

        //Проверка клик по узлу
        var clickedHandle = _renderer.GetHandleAt(pt);
        if (clickedHandle != null && SelectedModel != null)
        {
            var handleIndex = _renderer.GetHandleIndex(clickedHandle);
            var polylineIndex = _drawing.Polylines.IndexOf(SelectedModel);

            if (polylineIndex >= 0 && handleIndex >= 0 && handleIndex < SelectedModel.Points.Count)
            {
                _draggedPolylineNodeIndex = polylineIndex;
                _draggedPointIndex = handleIndex;
                _dragOffsetForNode = pt - SelectedModel.Points[handleIndex];
                return;
            }
        }


        //Проверка клик по ломаной
        var clickedPolyline = _renderer.GetPolylineAt(pt);
        if (clickedPolyline != null)
        {
            if (clickedHandle == null)
            {
                var polylineIndex = _renderer.UIPolylines.IndexOf(clickedPolyline);
                if (polylineIndex >= 0 && polylineIndex < _drawing.Polylines.Count)
                {
                    var model = _drawing.Polylines[polylineIndex];
                    if (model.Points.Count > 0)
                    {
                        var referencePoint = model.Points[0];
                        _draggedPolylineIndex = polylineIndex;
                        _dragOffsetForPolyline = pt - referencePoint;
                        SelectPolyline(clickedPolyline);
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        //Клик на пустом месте
        ClearSelection();
    }

    public void HandleMouseRightBtnDown(string color, double thickness, MouseButtonEventArgs e)
    {
        if (IsDrawingNew)
        {
            TryFinishNewPolyline(color, thickness);
            e.Handled = true;
            Mouse.OverrideCursor = null;
            _canvas.Focus();
        }
    }

    public void HandleMouseMove(Point pt, bool isLeftPressed)
    {
        // Перетаскивание узла
        if (_draggedPolylineNodeIndex >= 0 && _draggedPointIndex >= 0 && isLeftPressed && _dragOffsetForNode.HasValue)
        {
            var model = _drawing.Polylines[_draggedPolylineNodeIndex];
            if (_draggedPointIndex < model.Points.Count)
            {
                var newPosition = pt - _dragOffsetForNode.Value;
                model.Points[_draggedPointIndex] = newPosition;

                if (_draggedPolylineNodeIndex < _renderer.UIPolylines.Count)
                {
                    var uiPolyline = _renderer.UIPolylines[_draggedPolylineNodeIndex];
                    if (_draggedPointIndex < uiPolyline.Points.Count)
                    {
                        uiPolyline.Points[_draggedPointIndex] = newPosition;
                    }
                }

                if (SelectedModel == model && _draggedPointIndex < _renderer.Handles.Count)
                {
                    var handle = _renderer.Handles[_draggedPointIndex];
                    Canvas.SetLeft(handle, newPosition.X - 4);
                    Canvas.SetTop(handle, newPosition.Y - 4);
                }
            }
            return;
        }

        // Перетаскивание всей линии
        if (_draggedPolylineIndex >= 0 && isLeftPressed && _dragOffsetForPolyline.HasValue)
        {
            var model = _drawing.Polylines[_draggedPolylineIndex];
            var uiPolyline = _renderer.UIPolylines[_draggedPolylineIndex];

            var newReferencePoint = pt - _dragOffsetForPolyline.Value;

            var oldReferencePoint = model.Points[0];

            var delta = newReferencePoint - oldReferencePoint;

            for (int i = 0; i < model.Points.Count; i++)
            {
                model.Points[i] = new Point(model.Points[i].X + delta.X, model.Points[i].Y + delta.Y);
                uiPolyline.Points[i] = model.Points[i];
            }

            if (SelectedModel == model)
            {
                for (int i = 0; i < _renderer.Handles.Count && i < model.Points.Count; i++)
                {
                    var handle = _renderer.Handles[i];
                    var p = model.Points[i];
                    Canvas.SetLeft(handle, p.X - 4);
                    Canvas.SetTop(handle, p.Y - 4);
                }
            }

            return;
        }
        // Рисование временной линии
        if (IsDrawingNew && isLeftPressed && _currentPoints.Count > 0)
        {
            var temp = new List<Point>(_currentPoints) { pt };
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
            return;
        }
    }

    public void HandleMouseUp(Point pt)
    {
        _draggedPolylineNodeIndex = -1;
        _draggedPointIndex = -1;
        _dragOffsetForNode = null;

        _draggedPolylineIndex = -1;
        _dragOffsetForPolyline = null;
    }

    public void HandleDoubleClickInternal(Point pt)
    {
        if (SelectedModel == null)
            return;

        var result = GeometryHelper.FindNearestSegmentInsertion(new PointCollection(SelectedModel.Points), pt);

        if (result.HasValue)
        {
            SelectedModel.Points.Insert(result.Value.segmentIndex, result.Value.projection);
            RefreshDrawing();
            SelectPolylineByModel(SelectedModel);
        }
    }

    public void DeleteSelected()
    {
        if (SelectedModel == null) return;

        _drawing.Polylines.Remove(SelectedModel);
        SelectedModel = null;
        RefreshDrawing();
        _renderer.ClearHandles();
    }

    public void DeleteAll()
    {
        if (_drawing.Polylines.Count == 0) return;
        _drawing.Polylines.Clear();
        SelectedModel = null;
        RefreshDrawing();
        _renderer.ClearHandles();
    }

    private void SelectPolyline(Polyline pl)
    {
        var index = _renderer.UIPolylines.IndexOf(pl);
        if (index >= 0 && index < _drawing.Polylines.Count)
        {
            SelectedModel = _drawing.Polylines[index];
            _renderer.ShowHandlesFor(SelectedModel);
            SelectedLineChanged?.Invoke();
        }
    }

    private void SelectPolylineByModel(PolylineModel model)
    {
        SelectedModel = model;
        _renderer.ShowHandlesFor(model);
    }

    public void ClearSelection()
    {
        SelectedModel = null;
        _renderer.ClearHandles();
    }

    private void RefreshDrawing()
    {
        _renderer.Redraw(_drawing);
        if (SelectedModel != null)
        {
            _renderer.ShowHandlesFor(SelectedModel);
        }
    }

    private void UpdateNewPolylinePreview()
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

        foreach (var p in _currentPoints)
            _currentNewPolyline.Points.Add(p);
    }

    public bool TryFinishNewPolyline(string color, double thickness)
    {
        if (!IsDrawingNew) return false;

        FinishNewPolyline(color, thickness);
        return true;
    }

    public void FinishNewPolyline(string color, double thickness)
    {
        if (_currentPoints.Count >= 2)
        {
            var model = new PolylineModel
            {
                Points = new List<Point>(_currentPoints),
                Color = color,
                Thickness = thickness
            };
            _drawing.Polylines.Add(model);

        }

        CancelNewPolyline();
        RefreshDrawing();
    }

    public void CancelNewPolyline()
    {
        IsDrawingNew = false;
        _currentPoints.Clear();

        if (_currentNewPolyline != null)
        {
            _canvas.Children.Remove(_currentNewPolyline);
            _currentNewPolyline = null;
        }
    }

    public void UpdateSelectedLineColor(string color)
    {
        if (SelectedModel != null)
        {
            SelectedModel.Color = color;
            RefreshDrawing();
        }
    }

    public void UpdateSelectedLineThickness(double thickness)
    {
        if (SelectedModel != null)
        {
            SelectedModel.Thickness = thickness;
            RefreshDrawing();
        }
    }

    public (string color, double thickness) GetSelectedLineProperties()
    {
        if (SelectedModel != null)
        {
            return (SelectedModel.Color, SelectedModel.Thickness);
        }
        return ("Black", 2.0);
    }

    

}
