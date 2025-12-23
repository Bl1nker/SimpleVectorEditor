using System.IO;
using System.Text.Json;
using VectorEditor.Models;

namespace VectorEditor.Services;

public class DrawingPersistenceService
{
    private readonly Func<string?> _getSaveFilePath;
    private readonly Func<string?> _getOpenFilePath;

    public DrawingPersistenceService(Func<string?> getSaveFilePath, Func<string?> getOpenFilePath)
    {
        _getSaveFilePath = getSaveFilePath;
        _getOpenFilePath = getOpenFilePath;
    }

    public DrawingModel LoadFromFile()
    {
        var path = _getOpenFilePath();
        if (path == null || !File.Exists(path))
        {
            return new DrawingModel();
        }

        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<DrawingModel>(json) ?? new DrawingModel();
    }

    public void SaveToFile(DrawingModel model)
    {
        var path = _getSaveFilePath();
        if (path == null) 
            return;

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(model, options);
        File.WriteAllText(path, json);
    }
}
