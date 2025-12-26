using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using VectorEditor.Models;
using VectorEditor.Rendering;

namespace VectorEditor.Services;

public class DrawingPersistence
{
    private readonly Polyline _polyline;
    private readonly VisualRenderer _renderer;

    public DrawingPersistence(Polyline polyline, VisualRenderer renderer)
    {
        _polyline = polyline;
        _renderer = renderer;
    }

    public void LoadFromFile()
    {
        var dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };

        if (dlg.ShowDialog() == true)
        {
            try
            {
                var json = File.ReadAllText(dlg.FileName);
                DrawingModel model = JsonSerializer.Deserialize<DrawingModel>(json) ?? new DrawingModel();

                _renderer.SetDrawingModel(model);

                _polyline.ClearSelection();
            }
            catch (IOException exc)
            {
                MessageBox.Show("Не удалось прочитать файл: " + exc.Message);
                return;
            }
        }
    }

    public void SaveToFile(DrawingModel model)
    {
        var dlg = new SaveFileDialog { Filter = "JSON files (*.json)|*.json" };
        if (dlg.ShowDialog() == true)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(model, options);
            File.WriteAllText(dlg.FileName, json);
        }        
    }
}
