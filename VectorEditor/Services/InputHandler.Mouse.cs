using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace VectorEditor.Services;

partial class InputHandler
{
    // Для определения двойного щелчка
    private DateTime _lastClickTime = DateTime.MinValue;
    private Point _lastClickPosition;
    private const double MaxDoubleClickTime = 400; // ms
    private const double MaxDoubleClickDistance = 10; // px


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
            HandleDoubleClick(pt);
            return;
        }

        // Обработка одинарного клика
        if (_polyline.draggedLineNodeIndex >= 0)
            return;

        //Рисуем новую линию
        if (_polyline.IsDrawingNew)
        {
            _polyline.AddPointToPolyline(pt);            
            return;
        }

        //Проверка клик по узлу
        var clickedNode = _renderer.GetNodeAt(pt);
        if (clickedNode != null && _polyline.SelectedModel != null)
        {
            var nodeIndex = _renderer.GetNodeIndex(clickedNode);
            var polylineIndex = _drawing.Polylines.IndexOf(_polyline.SelectedModel);

            if (polylineIndex >= 0 && nodeIndex >= 0 && nodeIndex < _polyline.SelectedModel.Points.Count)
            {
                _polyline.draggedLineNodeIndex = polylineIndex;
                _polyline.draggedPointIndex = nodeIndex;
                _polyline.dragNodeOffset = pt - _polyline.SelectedModel.Points[nodeIndex];
                return;
            }
        }

        //Проверка клик по линии
        var clickedLine = _renderer.GetLineAt(pt);
        if (clickedLine != null)
        {
            if (clickedNode == null)
            {
                var polylineIndex = _renderer.UILines.IndexOf(clickedLine);
                if (polylineIndex >= 0 && polylineIndex < _drawing.Polylines.Count)
                {
                    var model = _drawing.Polylines[polylineIndex];
                    if (model.Points.Count > 0)
                    {
                        var referencePoint = model.Points[0];
                        _polyline.draggedPolylineIndex = polylineIndex;
                        _polyline.dragPolylineOffset = pt - referencePoint;
                        _polyline.SelectPolyline(clickedLine);
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
        _polyline.ClearSelection();
    }

    public void HandleMouseRightBtnDown(MouseButtonEventArgs e)
    {
        if (_polyline.IsDrawingNew)
        {
            _polyline.TryFinishNewPolyline();
            e.Handled = true;
        }
    }

    public void HandleMouseMove(Point pt, bool isLeftPressed)
    {
        // Перетаскивание узла
        if (_polyline.draggedLineNodeIndex >= 0 && _polyline.draggedPointIndex >= 0 && isLeftPressed && _polyline.dragNodeOffset.HasValue)
        {
            _polyline.NodeMove(pt, _polyline.draggedLineNodeIndex, _polyline.draggedPointIndex, _polyline.dragNodeOffset);
            return;
        }

        // Перетаскивание всей линии
        if (_polyline.draggedPolylineIndex >= 0 && isLeftPressed && _polyline.dragPolylineOffset.HasValue)
        {
            _polyline.LineMove(pt, _polyline.draggedPolylineIndex, _polyline.dragPolylineOffset);
            return;
        }
        // Рисование временной линии
        if (_polyline.IsDrawingNew && isLeftPressed && _polyline.currentPoints.Count > 0)
        {
            _polyline.TempPolyline(pt);
            return;
        }
    }

    public void HandleMouseUp(Point pt)
    {
        _polyline.draggedLineNodeIndex = -1;
        _polyline.draggedPointIndex = -1;
        _polyline.dragNodeOffset = null;

        _polyline.draggedPolylineIndex = -1;
        _polyline.dragPolylineOffset = null;
    }

    private void HandleDoubleClick(Point pt)
    {
        if (_polyline.SelectedModel == null)
            return;

        var result = Helpers.Geometry.FindNearestSegmentInsertion(new PointCollection(_polyline.SelectedModel.Points), pt);

        if (result.HasValue)
        {
            _polyline.AddNewNode(result.Value.segmentIndex, result.Value.projection);
            
        }
    }
}
