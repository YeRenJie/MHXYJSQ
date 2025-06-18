using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static mhcj.Form1;

namespace mhcj
{
    public enum TimeRangeType
    {
        RealTime,
        Today,
        Yesterday,
        ThisWeek,
        ThisMonth,
        LastMonth,
        ThisYear,
        All
    }

    public partial class Form1 : Form
    {


        // 当前服务器数据 (新增)
        private Dictionary<string, ServerData> _serverDataDict = new Dictionary<string, ServerData>();
        private string _currentServerId;
        private TimeRangeType _currentTimeRange = TimeRangeType.RealTime; // 当前时间范围

        private System.Windows.Forms.Timer _saveTimer = new System.Windows.Forms.Timer();
        private bool _isTextChanged = false;
        private bool _isSwitchingServer = false;
        // 在Form1类中添加计时器事件处理方法
        private void ServerTimer_Tick(ServerData server)
        {
            try
            {
                // 更新当前服务器的累计时间
                server.ElapsedTime = server.ElapsedTime.Add(TimeSpan.FromSeconds(600));

                // 添加点卡记录
                server.ProfitItems.Insert(0, new ProfitItem
                {
                    Category = "点卡",
                    Price = decimal.Parse(server.DKText),
                    Quantity = int.Parse(server.RSText),
                    JSQuantity = int.Parse(server.RSText),
                    Time = DateTime.Now
                });

                // 保存数据
                DataManager.SaveData(server.ProfitItems.ToList(), server.ServerId);

                // 如果当前显示的是这个服务器，刷新UI
                if (server.ServerId == _currentServerId)
                {
                    // 使用Invoke确保跨线程安全
                    if (lbljs.InvokeRequired)
                    {
                        lbljs.Invoke(new Action(() =>
                        {
                            lbljs.Text = $"已在线：{server.ElapsedTime:hh\\:mm\\:ss}";
                            RefreshStatistics(server);
                        }));
                    }
                    else
                    {
                        lbljs.Text = $"已在线：{server.ElapsedTime:hh\\:mm\\:ss}";
                        RefreshStatistics(server);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"定时任务错误：{ex.Message}");
            }
        }
        public Form1()
        {
            InitializeComponent();

            // 初始化定时器（延迟500毫秒保存）
            _saveTimer.Interval = 500;
            _saveTimer.Tick += (s, e) =>
            {
                if (_isTextChanged)
                {
                    SaveToConfig();
                    _isTextChanged = false;
                }
                _saveTimer.Stop();
            };

            // 初始化服务器数据 (新增)
            InitializeServerData();

            // 加载默认服务器 (新增)
            LoadDefaultServer();

            // 初始化定时器
            timer1.Interval = 10 * 60 * 1000;
            timer1.Tick += Timer1_Tick;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var currentServer = _serverDataDict[_currentServerId];

            // 7. 刷新统计数据
            RefreshStatistics(currentServer);
        }
        // 初始化服务器数据 (新增)
        private void InitializeServerData()
        {
            // 加载所有区服数据
            var services = PriceManager.LoadService();
            foreach (var service in services)
            {
                var serverData = new ServerData(service.name);

                // 加载价格列表
                serverData.PriceList = new BindingList<PriceItem>(PriceManager.LoadPrices(service.name));
                serverData.DKText = service.dk.ToString();
                serverData.RSText = service.js.ToString();

                // 加载服务列表
                serverData.ServiceList = new BindingList<selectqu>(services);

                // 加载收益数据
                serverData.ProfitItems = new BindingList<ProfitItem>(DataManager.LoadData(service.name));
                serverData.ProfitItems.ListChanged += (s, e) =>
                {
                    if (e.ListChangedType == ListChangedType.ItemChanged ||
                        e.ListChangedType == ListChangedType.ItemAdded ||
                        e.ListChangedType == ListChangedType.ItemDeleted)
                    {
                        DataManager.SaveData(serverData.ProfitItems.ToList(), serverData.ServerId);
                    }
                };

                // 修复点卡数据
                foreach (var item in serverData.ProfitItems)
                {
                    if (item.Category == "点卡" && item.JSQuantity <= 0)
                    {
                        var sjsl = DivideAndRound(item.Price * item.Quantity, decimal.Parse(serverData.DKText));
                        item.Price = decimal.Parse(serverData.DKText);
                        item.JSQuantity = (int)Math.Round(sjsl) > int.Parse(serverData.RSText) ?
                            int.Parse(serverData.RSText) : (int)Math.Round(sjsl);
                        item.Quantity = (int)Math.Round(sjsl);
                    }
                }

                // 为每个服务器设置计时器事件
                serverData.ServerTimer.Tick += (s, e) => ServerTimer_Tick(serverData);

                _serverDataDict.Add(service.name, serverData);
            }
        }


        // 加载默认服务器 (新增)
        private void LoadDefaultServer()
        {
            if (_serverDataDict.Count > 0)
            {
                _currentServerId = _serverDataDict.Keys.First();
                SwitchServer(_currentServerId);
            }
        }

        // 切换服务器 (新增)
        private void SwitchServer(string serverId)
        {
            // 保存当前服务器状态
            if (!string.IsNullOrEmpty(_currentServerId))
            {
                SaveCurrentServerState();
            }

            // 切换到新服务器
            _currentServerId = serverId;
            var currentServer = _serverDataDict[_currentServerId];
            if (string.IsNullOrEmpty(currentServer.BtOk))
            {
                currentServer.BtOk = "开始计时";
                currentServer.StartTimer();
            }
            // 更新UI
            UpdateUIForCurrentServer(currentServer);
        }
        private void btnStopAllTimers_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要停止所有服务器的计时器吗？",
                   "确认停止", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (var server in _serverDataDict.Values)
                {
                    server.StopTimer();
                }

                // 更新当前服务器按钮状态
                if (_serverDataDict.ContainsKey(_currentServerId))
                {
                    var currentServer = _serverDataDict[_currentServerId];
                    currentServer.IsTimerRunning = false;
                    btnjs.Text = "开始";
                }

                MessageBox.Show("所有计时器已停止！");
            }
        }
        // 保存当前服务器状态 (新增)
        private void SaveCurrentServerState()
        {
            var currentServer = _serverDataDict[_currentServerId];

            // 保存点卡和人数设置
            currentServer.DKText = txtdk.Text;
            currentServer.RSText = txtjs.Text;

            // 保存计时器状态
            currentServer.IsTimerRunning = (btnjs.Text == "暂停");

            // 保存选中项
            currentServer.SelectedPriceItem = (PriceItem)comboBox1.SelectedItem;
            currentServer.SelectedService = (selectqu)comboBox2.SelectedItem;
        }

        // 更新UI (重构)
        private void UpdateUIForCurrentServer(ServerData serverData)
        {
            // 1. 更新点卡和人数
            txtdk.Text = serverData.DKText;
            txtjs.Text = serverData.RSText;

            // 2. 更新价格下拉框
            comboBox1.DataSource = null;
            comboBox1.DataSource = serverData.PriceList;
            comboBox1.DisplayMember = "Category";
            comboBox1.ValueMember = "Price";
            RefreshComboBox();

            // 恢复选中项
            if (serverData.SelectedPriceItem != null &&
                serverData.PriceList.Contains(serverData.SelectedPriceItem))
            {
                comboBox1.SelectedItem = serverData.SelectedPriceItem;
            }
            else if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }

            // 3. 更新服务下拉框
            comboBox2.DataSource = null;
            comboBox2.DataSource = serverData.ServiceList;
            comboBox2.DisplayMember = "name";
            comboBox2.ValueMember = "js";
            comboBox2.SelectedItem = serverData.ServiceList.FirstOrDefault(s => s.name == serverData.ServerId);

            // 4. 更新数据网格
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = serverData.ProfitItems;

            // 更新在线时间显示
            lbljs.Text = $"已在线：{serverData.ElapsedTime:hh\\:mm\\:ss}";
            lblqdsj.Text = $"开始时间: {serverData.StartTime:yyyyMMdd HH:mm}";
            // 更新按钮状态
            btnjs.Text = serverData.IsTimerRunning ? "暂停" : "开始";



            // 7. 刷新统计数据
            RefreshStatistics(serverData);

            // 8. 更新快速添加按钮
            UpdateButtonsForCurrentServer(serverData);

            // 9. 更新时间范围选择
            UpdateTimeRangeUI();
        }

        // 更新时间范围UI (新增)
        private void UpdateTimeRangeUI()
        {
            // 根据当前时间范围设置单选按钮
            switch (_currentTimeRange)
            {
                case TimeRangeType.RealTime: radioButton7.Checked = true; break;
                case TimeRangeType.Today: radioButton8.Checked = true; break;
                case TimeRangeType.Yesterday: radioButton6.Checked = true; break;
                case TimeRangeType.ThisWeek: radioButton1.Checked = true; break;
                case TimeRangeType.ThisMonth: radioButton2.Checked = true; break;
                case TimeRangeType.LastMonth: radioButton3.Checked = true; break;
                case TimeRangeType.ThisYear: radioButton4.Checked = true; break;
                case TimeRangeType.All: radioButton5.Checked = true; break;
            }
        }

        // 更新快速添加按钮 (重构)
        private void UpdateButtonsForCurrentServer(ServerData serverData)
        {
            // 清除现有按钮
            flowPanel.Controls.Clear();

            // 添加新按钮
            foreach (var item in serverData.PriceList)
            {
                CreateItemButton(item, serverData);
            }
        }

        // 创建物品按钮 (重构)
        private void CreateItemButton(PriceItem item, ServerData serverData)
        {
            Color ys = Color.AliceBlue;
            if (item.Category == "点卡")
            {
                ys = Color.Red;
            }
            else if (item.Price < 100)
            {
                ys = Color.Green;
            }
            else if (item.Price < 500)
            {
                ys = Color.Orange;
            }
            else
            {
                ys = Color.Purple;
            }

            Button btn = new Button
            {
                ForeColor = Color.White,
                Text = item.Category,
                Tag = item,
                Margin = new Padding(6),
                BackColor = ys,
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.Popup
            };

            btn.Click += (s, e) =>
            {
                serverData.ProfitItems.Insert(0, new ProfitItem
                {
                    Category = item.Category,
                    Price = item.Price,
                    Quantity = 1,
                    Time = DateTime.Now,
                    JSQuantity = int.Parse(serverData.RSText)
                });
                RefreshStatistics(serverData);
            };

            flowPanel.Controls.Add(btn);
        }

        // 数据变动事件处理 (重构)
        private void ProfitItems_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged ||
                e.ListChangedType == ListChangedType.ItemAdded ||
                e.ListChangedType == ListChangedType.ItemDeleted)
            {
                var currentServer = _serverDataDict[_currentServerId];
                DataManager.SaveData(currentServer.ProfitItems.ToList(), currentServer.ServerId);
            }
        }

        // 添加新记录 (重构)
        private void button2_Click(object sender, EventArgs e)
        {
            var currentServer = _serverDataDict[_currentServerId];

            var newItem = new ProfitItem
            {
                Category = comboBox1.Text.Trim(),
                Price = decimal.Parse(txtJg.Text ?? "0"),
                Quantity = int.Parse(txtsl.Text == "" ? "0" : txtsl.Text),
                JSQuantity = int.Parse(txtjs.Text),
                Time = DateTime.Now
            };

            // 输入验证
            if (string.IsNullOrWhiteSpace(newItem.Category))
            {
                MessageBox.Show("类别不能为空");
                return;
            }

            if (newItem.Price <= 0 || newItem.Quantity <= 0)
            {
                MessageBox.Show("价格和数量必须大于0");
                return;
            }

            currentServer.ProfitItems.Insert(0, newItem);
            RefreshStatistics(currentServer);
        }

        // 修改价格 (重构)
        private void button3_Click(object sender, EventArgs e)
        {
            var currentServer = _serverDataDict[_currentServerId];

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("请先选择要修改的类别");
                return;
            }

            if (!decimal.TryParse(txtJg.Text, out var newPrice) || newPrice <= 0)
            {
                MessageBox.Show("请输入有效的正数价格");
                return;
            }

            if (MessageBox.Show($"确定要修改【{((PriceItem)comboBox1.SelectedItem).Category}】的价格吗？",
                "确认修改", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            var selectedItem = (PriceItem)comboBox1.SelectedItem;
            selectedItem.Price = newPrice;

            // 刷新绑定
            var index = comboBox1.SelectedIndex;
            comboBox1.DataSource = null;
            comboBox1.DataSource = currentServer.PriceList;
            comboBox1.DisplayMember = "Category";
            comboBox1.ValueMember = "Price";
            comboBox1.SelectedIndex = index;

            // 保存到文件
            PriceManager.SavePrices(currentServer.PriceList.ToList(), currentServer.ServerId);

            RefreshStatistics(currentServer);
        }

        // 刷新统计数据 (重构)
        private void RefreshStatistics(ServerData serverData)
        {
            DateTime startTime, endTime;
            CalculateTimeRange(serverData, out startTime, out endTime);

            // 计算点卡消耗
            decimal monthProfit = serverData.ProfitItems
                .Where(item => item.Category == "点卡" &&
                      item.Time >= startTime &&
                      item.Time <= endTime)
                .Sum(item => item.ItemProfit);

            decimal monthSj = serverData.ProfitItems
                .Where(item => item.Category == "点卡" &&
                      item.Time >= startTime &&
                      item.Time <= endTime)
                .Sum(item => DivideAndRound(item.Quantity, (item.JSQuantity == 0 ? item.Quantity : item.JSQuantity)));

            // 计算收益
            decimal dysy = serverData.ProfitItems
                .Where(item => item.Category != "点卡" &&
                      item.Time >= startTime &&
                      item.Time <= endTime)
                .Sum(item => item.ItemProfit);

            var dktotal = Math.Abs(monthProfit);
            var dkxs = DivideAndRound(monthSj * 10, 60);
            lbldyzx.Text = $"{dkxs:N1} 小时";
            lbldydk.Text = $"{dktotal:N0} W / {dktotal / (decimal.Parse(txtdk.Text) * 10):N0}￥";
            lbldysy.Text = $"{dysy:N0} W / {dysy / (decimal.Parse(txtdk.Text) * 10):N0}￥";
            lbldylr.Text = $"{(dysy - dktotal):N0} W / {(dysy - dktotal) / (decimal.Parse(txtdk.Text) * 10):N0}￥";

            // 统计各物品数量
            label11.Text = string.Empty;
            label13.Text = string.Empty;

            var categoryCounts = serverData.ProfitItems
                .Where(item => item.Category != "点卡" &&
                      item.Time >= startTime &&
                      item.Time <= endTime)
                .GroupBy(item => item.Category)
                .OrderByDescending(a => a.Key)
                .ToDictionary(g => g.Key, g => g.Sum(item => item.Quantity));

            foreach (var item in categoryCounts)
            {
                if (item.Key.Contains("环"))
                {
                    label13.Text += $"{item.Key}:{item.Value}   ";
                }
                else
                {
                    label11.Text += $"{item.Key}:{item.Value}   ";
                }
            }

            // 刷新数据网格
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = serverData.ProfitItems;
        }

        // 计算时间范围 (重构)
        private void CalculateTimeRange(ServerData serverData, out DateTime startTime, out DateTime endTime)
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            switch (_currentTimeRange)
            {
                case TimeRangeType.All:
                    startTime = DateTime.MinValue;
                    endTime = now;
                    break;

                case TimeRangeType.RealTime:
                    startTime = serverData.StartTime;
                    endTime = now;
                    break;

                case TimeRangeType.Today:
                    startTime = today;
                    endTime = today.AddDays(1).AddSeconds(-1);
                    break;

                case TimeRangeType.Yesterday:
                    startTime = today.AddDays(-1);
                    endTime = today.AddSeconds(-1);
                    break;

                case TimeRangeType.ThisWeek:
                    int diffToMonday = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var weekStart = today.AddDays(-diffToMonday);
                    startTime = weekStart;
                    endTime = weekStart.AddDays(7).AddSeconds(-1);
                    break;

                case TimeRangeType.ThisMonth:
                    var monthStart = new DateTime(today.Year, today.Month, 1);
                    startTime = monthStart;
                    endTime = monthStart.AddMonths(1).AddSeconds(-1);
                    break;

                case TimeRangeType.LastMonth:
                    var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
                    startTime = firstDayThisMonth.AddMonths(-1);
                    endTime = firstDayThisMonth.AddSeconds(-1);
                    break;

                case TimeRangeType.ThisYear:
                    var yearStart = new DateTime(today.Year, 1, 1);
                    startTime = yearStart;
                    endTime = yearStart.AddYears(1).AddSeconds(-1);
                    break;

                default:
                    startTime = serverData.StartTime;
                    endTime = now;
                    break;
            }
        }

        // 清除数据 (重构)
        private void btnClearData_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空当前服务器的所有数据吗？此操作不可恢复！",
                   "确认清空", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var currentServer = _serverDataDict[_currentServerId];

                // 清空数据
                currentServer.ProfitItems.Clear();
                DataManager.ClearData(currentServer.ServerId);

                // 重置开始时间
                currentServer.StartTime = DateTime.Now;
                currentServer.ElapsedTime = TimeSpan.Zero;
                lblqdsj.Text = $"开始时间: {currentServer.StartTime:yyyyMMdd HH:mm}";
                lbljs.Text = $"已在线：{currentServer.ElapsedTime:hh\\:mm\\:ss}";

                // 刷新界面
                RefreshStatistics(currentServer);
                MessageBox.Show("数据已清空！");
            }
        }
        public void RefreshComboBox()
        {
            if (comboBox1.SelectedItem is PriceItem selectedItem)
            {
                // 显示当前选中项的价格
                txtJg.Text = selectedItem.Price.ToString();

                // 或者通过ValueMember获取
                // var price = comboBox1.SelectedValue;
            }
        }
        // 设置价格表 (重构)
        private void 设置价格表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 准备所有服务器的价格数据
            var serverPrices = new Dictionary<string, BindingList<PriceItem>>();

            foreach (var server in _serverDataDict)
            {
                serverPrices.Add(server.Key, server.Value.PriceList);
            }

            var priceEditor = new 价格(serverPrices, _currentServerId, this);
            priceEditor.ShowDialog();

            // 刷新当前服务器的价格列表
            var currentServer = _serverDataDict[_currentServerId];
            comboBox1.DataSource = null;
            comboBox1.DataSource = currentServer.PriceList;
            comboBox1.DisplayMember = "Category";
            comboBox1.ValueMember = "Price";

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            // 刷新快速添加按钮
            UpdateButtonsForCurrentServer(currentServer);
        }

        // 计时器控制 (重构)
        private void 计时_Click(object sender, EventArgs e)
        {
            if (!_serverDataDict.ContainsKey(_currentServerId)) return;

            var currentServer = _serverDataDict[_currentServerId];

            if (btnjs.Text == "开始")
            {
                currentServer.StartTimer();
                btnjs.Text = "暂停";
            }
            else
            {
                currentServer.StopTimer();
                btnjs.Text = "开始";
            }
        }

        // 定时器事件 (重构)
        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                var currentServer = _serverDataDict[_currentServerId];

                // 更新累计时间
                currentServer.ElapsedTime = currentServer.ElapsedTime.Add(TimeSpan.FromSeconds(600));
                UpdateTimeLabel(currentServer.ElapsedTime);

                // 添加点卡记录
                currentServer.ProfitItems.Insert(0, new ProfitItem
                {
                    Category = "点卡",
                    Price = decimal.Parse(currentServer.DKText),
                    Quantity = int.Parse(currentServer.RSText),
                    JSQuantity = int.Parse(currentServer.RSText),
                    Time = DateTime.Now
                });

                RefreshStatistics(currentServer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"定时任务错误：{ex.Message}");
            }
        }

        // 更新计时标签 (重构)
        private void UpdateTimeLabel(TimeSpan time)
        {
            lbljs.Text = $"已在线：{time:hh\\:mm\\:ss}";
        }

        // 保存配置 (重构)
        private void SaveToConfig()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentServerId))
                {
                    var currentServer = _serverDataDict[_currentServerId];
                    currentServer.DKText = txtdk.Text;
                    currentServer.RSText = txtjs.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败：{ex.Message}");
            }
        }

        // 切换服务器事件 (新增)
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSwitchingServer || comboBox2.SelectedItem == null)
                return;

            try
            {
                _isSwitchingServer = true;
                var selectedService = (selectqu)comboBox2.SelectedItem;

                if (selectedService.name != _currentServerId)
                {
                    SwitchServer(selectedService.name);
                }
            }
            finally
            {
                _isSwitchingServer = false;
            }
        }

        // 时间范围切换事件 (重构)
        private void TimeRange_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked) _currentTimeRange = TimeRangeType.RealTime;
            else if (radioButton8.Checked) _currentTimeRange = TimeRangeType.Today;
            else if (radioButton6.Checked) _currentTimeRange = TimeRangeType.Yesterday;
            else if (radioButton1.Checked) _currentTimeRange = TimeRangeType.ThisWeek;
            else if (radioButton2.Checked) _currentTimeRange = TimeRangeType.ThisMonth;
            else if (radioButton3.Checked) _currentTimeRange = TimeRangeType.LastMonth;
            else if (radioButton4.Checked) _currentTimeRange = TimeRangeType.ThisYear;
            else if (radioButton5.Checked) _currentTimeRange = TimeRangeType.All;

            if (!string.IsNullOrEmpty(_currentServerId))
            {
                RefreshStatistics(_serverDataDict[_currentServerId]);
            }
        }

        // 辅助方法：安全除法 (保留)
        public static decimal DivideAndRound(decimal dividend, decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("除数不能为零。");

            decimal quotient = dividend / divisor;
            return Math.Round(quotient, 2, MidpointRounding.AwayFromZero);
        }

        // 其他事件处理 (保留)
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Time" && e.Value is DateTime dt)
            {
                e.Value = dt.ToString("MM-dd HH:mm");
                e.FormattingApplied = true;
            }
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void txtSl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) button2.PerformClick();
        }

        private void txtdk_TextChanged(object sender, EventArgs e)
        {
            _isTextChanged = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void txtjs_TextChanged(object sender, EventArgs e)
        {
            _isTextChanged = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            // 停止所有服务器的计时器
            foreach (var server in _serverDataDict.Values)
            {
                server.StopTimer();
            }

        }
    }
}