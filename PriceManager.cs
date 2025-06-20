using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace mhcj
{
    public static class PriceManager
    {
        private static readonly string ConfigPath =
            Path.Combine(Environment.CurrentDirectory, "price_list.json");
        // 加载服务器列表
        public static BindingList<selectqu> LoadService()
        {
            try
            {
                string quPath =
           Path.Combine(Environment.CurrentDirectory, "qu_list.json");
                if (File.Exists(quPath))
                {
                    string json = File.ReadAllText(quPath);
                    return JsonConvert.DeserializeObject<BindingList<selectqu>>(json);
                }

                var defaultList = new BindingList<selectqu>
        {
            new selectqu(){dk=1.42M,js=5, name="默认区"},
        };
                string json1 = JsonConvert.SerializeObject(defaultList, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(quPath, json1);
                Console.WriteLine("保存成功！");
                return defaultList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}");
                return null;
            }
        }
        // 保存服务器列表
        public static void SaveService(BindingList<selectqu> defaultList)
        {
            try
            {
                string quPath =
           Path.Combine(Environment.CurrentDirectory, "qu_list.json");
                string json = JsonConvert.SerializeObject(defaultList, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(quPath, json);
                Console.WriteLine("保存成功！");
             
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}");

            }
        }
        public static List<PriceItem> LoadPrices(string serverId)
        {
            string filePath = GetPriceFilePath(serverId);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<PriceItem>>(json) ?? new List<PriceItem>();
            }
            //  CreateDefaultList();
            return CreateDefaultList(serverId).ToList();
        }

        public static void SavePrices(List<PriceItem> prices, string serverId)
        {
            string filePath = GetPriceFilePath(serverId);
            var json = JsonConvert.SerializeObject(prices, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private static string GetPriceFilePath(string serverId)
        {
            string directory = Path.Combine(Application.StartupPath, "Prices");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return Path.Combine(directory, $"{serverId}_prices.json");
        }

        // 创建默认列表
        private static BindingList<PriceItem> CreateDefaultList(string serverId)
        {
            string json = @"[
          {
            ""Category"": ""谛听"",
            ""Price"": 1050
          },
          {
            ""Category"": ""广目"",
            ""Price"": 1800
          },
          {
            ""Category"": ""持国"",
            ""Price"": 960
          },
          {
            ""Category"": ""多闻"",
            ""Price"": 960
          },
          {
            ""Category"": ""龙龟"",
            ""Price"": 960
          },
          {
            ""Category"": ""眼镜妹"",
            ""Price"": 960
          },
          {
            ""Category"": ""天狗"",
            ""Price"": 800
          },
          {
            ""Category"": ""5J童子"",
            ""Price"": 600
          },
          {
            ""Category"": ""4J童子"",
            ""Price"": 130
          },
          {
            ""Category"": ""天女"",
            ""Price"": 100
          },
          {
            ""Category"": ""变异"",
            ""Price"": 180
          },
          {
            ""Category"": ""80环【武】"",
            ""Price"": 62
          },
          {
            ""Category"": ""80环【装】"",
            ""Price"": 50
          },
          {
            ""Category"": ""70环【武】"",
            ""Price"": 12.5
          },
          {
            ""Category"": ""70环【装】"",
            ""Price"": 9.5
          },
          {
            ""Category"": ""60环【武】"",
            ""Price"": 23
          },
          {
            ""Category"": ""60环【装】"",
            ""Price"": 20
          }
        ]";
            var defaultList = JsonConvert.DeserializeObject<BindingList<PriceItem>>(json);


            SavePrices(defaultList.ToList(), serverId);
            return defaultList;
        }
    }
}
