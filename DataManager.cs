using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mhcj
{
    public static class DataManager
    {
        private static readonly string DataPath =
            Path.Combine(Environment.CurrentDirectory, "profit_data.json");
        // 添加清空数据方法
        public static void ClearData()
        {
            try
            {
                if (File.Exists(DataPath))
                {
                    File.Delete(DataPath);
                    Console.WriteLine("数据文件已删除！");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清空失败：{ex.Message}");
            }
        }
        // 保存数据
        public static void SaveData(List<ProfitItem> data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(DataPath, json);
                Console.WriteLine("数据保存成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存失败：{ex.Message}");
            }
        }

        // 加载数据
        public static List<ProfitItem> LoadData()
        {
            try
            {
                if (File.Exists(DataPath))
                {
                    string json = File.ReadAllText(DataPath);
                    return JsonConvert.DeserializeObject<List<ProfitItem>>(json).OrderByDescending(a=>a.Time).ToList();
                }
                return new List<ProfitItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载失败：{ex.Message}");
                return new List<ProfitItem>();
            }
        }
    }
}
