using System.Windows;
using System.Windows.Input;
using VectorEditor.Models;

namespace VectorEditor.Services;

partial class InputHandler
{
    // Для определения двойного клика
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
        if (_shape.draggedShapeNodeIndex >= 0)
            return;

        //Рисуем новую линию
        if (_shape.IsDrawingNewPolyline)
        {
            _shape.AddPointToPolyline(pt);
            return;
        }

        //Проверка клик по узлу
        var clickedNode = _renderer.GetNodeAt(pt);
        if (clickedNode != null)
        {
            if (_shape.SelectedModel is PolylineModel pline)
            {
                var nodeIndex = _renderer.GetNodeIndex(clickedNode);
                var polylineIndex = _drawing.Elements.IndexOf(pline);

                if (polylineIndex >= 0 && nodeIndex >= 0 && nodeIndex < pline.Points.Count)
                {
                    _shape.draggedShapeNodeIndex = polylineIndex;
                    _shape.draggedPointIndex = nodeIndex;
                    _shape.dragNodeOffset = pt - pline.Points[nodeIndex];
                    return;
                }
            }
        }

        //Проверка клик по объекту
        var clickedObject = _renderer.GetElementAt(pt);
        if (clickedObject != null)
        {
            if (clickedNode == null)
            {
                var objectIndex = _renderer.UIShapes.IndexOf(clickedObject);

                if (objectIndex >= 0 && objectIndex < _drawing.Elements.Count)
                {
                    var model = _drawing.Elements[objectIndex];

                    if (model is PolylineModel line && line.Points.Count > 0)
                    {
                        var referencePoint = line.Points[0];
                        _shape.draggedShapeIndex = objectIndex;
                        _shape.dragShapeOffset = pt - referencePoint;
                        _shape.SelectObject(clickedObject);
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
        _shape.ClearSelection();
    }

    public void HandleMouseRightBtnDown(MouseButtonEventArgs e)
    {
        if (_shape.IsDrawingNewPolyline)
        {
            _shape.TryFinishNewPolyline();
            e.Handled = true;
        }
    }

    public void HandleMouseMove(Point pt, bool isLeftPressed)
    {
        // Перетаскивание узла
        if (_shape.draggedShapeNodeIndex >= 0 && _shape.draggedPointIndex >= 0 && isLeftPressed && _shape.dragNodeOffset.HasValue)
        {
            _shape.NodeMove(pt);
            return;
        }

        // Перетаскивание всего объекта
        if (_shape.draggedShapeIndex >= 0 && isLeftPressed && _shape.dragShapeOffset.HasValue)
        {
            if (_shape.SelectedModel is PolylineModel)
            {
                _shape.LineMove(pt);
                return;
            }
        }
        // Рисование временной полилинии
        if (_shape.IsDrawingNewPolyline && isLeftPressed && _shape.currentPoints.Count > 0)
        {
            _shape.TempPolyline(pt);
            return;
        }
    }

    public void HandleMouseUp()
    {
        _shape.draggedShapeNodeIndex = -1;
        _shape.draggedPointIndex = -1;
        _shape.dragNodeOffset = null;

        _shape.draggedShapeIndex = -1;
        _shape.dragShapeOffset = null;
    }

    private void HandleDoubleClick(Point pt)
    {
        if (_shape.SelectedModel == null)
            return;

        if (_shape.SelectedModel is PolylineModel)
        {
            _shape.AddNewNode(pt);
        }
    }
}
