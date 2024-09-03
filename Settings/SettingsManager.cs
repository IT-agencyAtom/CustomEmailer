using System.IO;
using Newtonsoft.Json;

namespace CustomEmailer.Settings;

public class SettingsManager<T>(string fileName) where T : class
{
    public T? LoadSettings() =>
        File.Exists(fileName) ?
            JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName)) :
            null;

    public void SaveSettings(T settings)
    {
        var json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(fileName, json);
    }
}