using System.Windows;

namespace VectorEditor.Models
{
    /// <summary>
    /// Модель полилинии
    /// </summary>
    public class PolylineModel
    {
        /// <summary>
        /// Точки полилинии
        /// </summary>
        public List<Point> Points { get; set; } = new();

        /// <summary>
        /// Толщина полилинии (по умолчанию 2.0).
        /// </summary>
        public double Thickness { get; set; } = 2.0;

        /// <summary>
        /// Цвет полилинии (по умолчанию Black)
        /// </summary>
        public string Color { get; set; } = "Black";
    }
}
