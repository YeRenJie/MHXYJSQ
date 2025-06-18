using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace mhcj
{
    // 数据模型类
    public class ProfitItem
    {
        [DisplayName("物品类别")]
        public string Category { get; set; }
        [DisplayName("单价")]
        public decimal Price { get; set; }
        [DisplayName("数量")]
        public int Quantity { get; set; }
        [DisplayName("添加时间")]
        public DateTime Time { get; set; } = DateTime.Now;
        [DisplayName("收益")]
        public decimal ItemProfit =>Category=="点卡"? -Price* Quantity: Price * Quantity;
        [DisplayName("角色数量")]
        public int JSQuantity { get; set; }
    }
}
