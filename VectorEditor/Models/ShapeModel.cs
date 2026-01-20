using Newtonsoft.Json;

namespace VectorEditor.Models;

/// <summary>
/// Базовая модель объекта
/// </summary>    
public abstract class ShapeModel
{
    /// <summary>
    /// Толщина объекта
    /// </summary>
    [JsonProperty]
    public double Thickness { get; set; } = 2.0;

    /// <summary>
    /// Цвет объекта
    /// </summary>
    [JsonProperty]
    public string Color { get; set; } = "Black";
}
