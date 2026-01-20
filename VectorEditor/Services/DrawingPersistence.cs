using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using VectorEditor.Models;

namespace VectorEditor.Services;

public static class DrawingPersistence
{
    public static DrawingModel? LoadFromFile()
    {
        var dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
        DrawingModel model;

        if (dlg.ShowDialog() == true)
        {
            try
            {
                var json = File.ReadAllText(dlg.FileName);
                model = JsonConvert.DeserializeObject<DrawingModel>(json, _settings) ?? new DrawingModel();

                return model;
            }
            catch (IOException exc)
            {
                MessageBox.Show("Не удалось прочитать файл: " + exc.Message);
            }
        }

        return null;
    }

    public static void SaveToFile(DrawingModel model)
    {
        var dlg = new SaveFileDialog { Filter = "JSON files (*.json)|*.json" };
        if (dlg.ShowDialog() == true)
        {            
            var json = JsonConvert.SerializeObject(model, _settings);
            File.WriteAllText(dlg.FileName, json);
        }
    }

    private static readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore        
    };
}
