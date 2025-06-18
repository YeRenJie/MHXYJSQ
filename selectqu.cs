using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace mhcj
{
    // 数据模型类

    public class selectqu
    {
        [DisplayName("自定义")]
        public string name { get; set; }
        [DisplayName("点卡")]
        public decimal dk { get; set; }
        [DisplayName("角色数")]
        public int js { get; set; }
        [DisplayName("服务器")]
        public string Server { get; set; }  // 新增服务器字段

    }
}
