using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace mhcj
{
    public static class PriceManager
    {
        private static readonly string ConfigPath =
            Path.Combine(Environment.CurrentDirectory, "price_list.json");

        // 加载价格列表
        public static BindingList<PriceItem> LoadPrices()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<BindingList<PriceItem>>(json);
                }
                return CreateDefaultList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载失败: {ex.Message}");
                return CreateDefaultList();
            }
        }

        // 保存价格列表
        public static void SavePrices(BindingList<PriceItem> prices)
        {
            try
            {
                string json = JsonConvert.SerializeObject(prices, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
                Console.WriteLine("保存成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存失败: {ex.Message}");
            }
        }

        // 创建默认列表
        private static BindingList<PriceItem> CreateDefaultList()
        {
            var defaultList = new BindingList<PriceItem>
        {
            new PriceItem("80环", 10000),
            new PriceItem("70环", 8000),
            new PriceItem("60环", 6000)
        };
            SavePrices(defaultList);
            return defaultList;
        }
    }
}
