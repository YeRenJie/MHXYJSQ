using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace mhcj
{
    // 服务器数据管理类 (新增)
    public class ServerData
    {
        public string ServerId { get; set; }
        public BindingList<ProfitItem> ProfitItems { get; set; }
        public BindingList<PriceItem> PriceList { get; set; }
        public BindingList<selectqu> ServiceList { get; set; }
        public bool IsTimerRunning { get; set; }
        public string BtOk { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public string DKText { get; set; }
        public string RSText { get; set; }
        public PriceItem SelectedPriceItem { get; set; }
        public selectqu SelectedService { get; set; }
        public DateTime StartTime { get; set; } // 新增：服务器独立计时开始时间
        public System.Windows.Forms.Timer ServerTimer { get; set; }

        public ServerData(string serverId)
        {
            ServerId = serverId;
            ProfitItems = new BindingList<ProfitItem>();
            PriceList = new BindingList<PriceItem>();
            ServiceList = new BindingList<selectqu>();
            DKText = "1.55";
            RSText = "5";
            // 初始化计时器
            ServerTimer = new System.Windows.Forms.Timer();
            ServerTimer.Interval = 10 * 60 * 1000; // 10分钟
            IsTimerRunning = false;
            ElapsedTime = TimeSpan.Zero;
            StartTime = DateTime.Now;
        }
        public void StartTimer()
        {
            ServerTimer.Start();
            IsTimerRunning = true;
        }

        public void StopTimer()
        {
            ServerTimer.Stop();
            IsTimerRunning = false;
        }

    }
}
