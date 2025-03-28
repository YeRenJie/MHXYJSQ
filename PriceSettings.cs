using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mhcj
{
    public class PriceItem
    {
        private string _category;
        private decimal _price;

        public string Category
        {
            get => _category;
            set => _category = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("类别不能为空")
                : value;
        }

        public decimal Price
        {
            get => _price;
            set => _price = value >= 0
                ? value
                : throw new ArgumentException("价格不能为负数");
        }

        public PriceItem(string category, decimal price)
        {
            Category = category;
            Price = price;
        }

        // 无参构造函数用于JSON反序列化
        public PriceItem() { }
    }
}
