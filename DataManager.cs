// DataManager.cs
using mhcj;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

public static class DataManager
{
    public static List<ProfitItem> LoadData(string serverId)
    {
        string filePath = GetDataFilePath(serverId);
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<ProfitItem>>(json) ?? new List<ProfitItem>();
        }
        return new List<ProfitItem>();
    }

    public static void SaveData(List<ProfitItem> data, string serverId)
    {
        string filePath = GetDataFilePath(serverId);
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public static void ClearData(string serverId)
    {
        string filePath = GetDataFilePath(serverId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static string GetDataFilePath(string serverId)
    {
        string directory = Path.Combine(Application.StartupPath, "Data");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return Path.Combine(directory, $"{serverId}_data.json");
    }
}