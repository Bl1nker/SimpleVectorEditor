using Newtonsoft.Json;
using System.Windows;

namespace VectorEditor.Models;

/// <summary>
/// Модель полилинии
/// </summary>
public class PolylineModel : ShapeModel
{
    /// <summary>
    /// Точки полилинии
    /// </summary>
    [JsonProperty]
    public List<Point> Points { get; set; } = new();
}
