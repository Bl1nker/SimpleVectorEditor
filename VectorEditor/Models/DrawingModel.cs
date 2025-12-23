
namespace VectorEditor.Models
{
    /// <summary>
    /// Модель рабочего пространства
    /// </summary>
    public class DrawingModel
    {
        /// <summary>
        /// Список элементов в рабочем пространстве
        /// </summary>
        public List<PolylineModel> Polylines { get; set; } = new();
    }
}
