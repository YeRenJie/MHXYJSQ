using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace mhcj
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer _saveTimer = new System.Windows.Forms.Timer();
        private bool _isTextChanged = false;

        // 窗体类成员变量
        private BindingList<ProfitItem> _profitItems;
        private BindingList<PriceItem> _priceList;  // 来自价格管理器的数据

        public Form1()
        {
            InitializeComponent();
            // 初始化定时器（延迟500毫秒保存）
            _saveTimer.Interval = 500; // 用户停止输入500毫秒后保存
            _saveTimer.Tick += (s, e) =>
            {
                if (_isTextChanged)
                {
                    SaveToConfig();
                    _isTextChanged = false;
                }
                _saveTimer.Stop();
            };
            // 加载价格列表并绑定到下拉框
            _priceList = new BindingList<PriceItem>(PriceManager.LoadPrices());

            // 配置ComboBox显示方式
            comboBox1.DataSource = _priceList;
            comboBox1.DisplayMember = "Category";  // 显示类别名称
            comboBox1.ValueMember = "Price";       // 值对应价格
            if (comboBox1.SelectedItem is PriceItem selectedItem)
            {
                // 显示当前选中项的价格
                txtJg.Text = selectedItem.Price.ToString();

                // 或者通过ValueMember获取
                // var price = comboBox1.SelectedValue;
            }
            // 设置默认选中项
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            // 绑定选择事件
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;

            // 加载数据

            _profitItems = new BindingList<ProfitItem>(DataManager.LoadData());
            //var sortedItems = _profitItems.OrderByDescending(a => a.Time).ToList();
            //_profitItems = new BindingList<ProfitItem>(sortedItems);
            foreach (var item in _priceList)
            {

                CreateItemButton(item);
            }

            // 绑定DataGridView
            dataGridView1.DataSource = _profitItems;
            dataGridView1.AutoGenerateColumns = true;

            // 监听数据变动事件
            _profitItems.ListChanged += ProfitItems_ListChanged;

            // 初始化显示
            label4.Text = "收 益：0";
            button1_Click(null, null);
        }
        // 数据变动事件处理
        private void ProfitItems_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged ||
                e.ListChangedType == ListChangedType.ItemAdded ||
                e.ListChangedType == ListChangedType.ItemDeleted)
            {
                // 自动保存
                DataManager.SaveData(_profitItems.ToList());
            }
        }
        // 创建单个 ProfitItem 对应的按钮
        private void CreateItemButton(PriceItem item)
        {
            Color ys = Color.AliceBlue;
            // 根据价格设置按钮背景色
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
                Text = item.Category,          // 按钮显示类别名称
                Tag = item,                    // 绑定关联的 ProfitItem
                Margin = new Padding(6),// 边距
                BackColor = ys,
                Size = new Size(80, 25),// 按钮尺寸
                FlatStyle = FlatStyle.Popup// 扁平化样式（可选）
            };

            // 绑定点击事件：处理按钮操作（如删除或编辑）
            btn.Click += (s, e) =>
            {
                _profitItems.Insert(0, new ProfitItem { Category = item.Category, Price = item.Price, Quantity = 1, Time = DateTime.Now });


                button1_Click(null, null);

            };

            flowPanel.Controls.Add(btn);
            flowPanel.Controls.SetChildIndex(btn, 0); // 插入到顶部（与 Insert(0) 对应）
        }


        // ComboBox选择项变更事件
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem is PriceItem selectedItem)
            {
                // 显示当前选中项的价格
                txtJg.Text = selectedItem.Price.ToString();
                txtsl.Text = "1";
                // 或者通过ValueMember获取
                // var price = comboBox1.SelectedValue;
            }
        }

        //添加新记录
        private void button2_Click(object sender, EventArgs e)
        {
            var newItem = new ProfitItem
            {
                Category = comboBox1.Text.Trim(),
                Price = decimal.Parse(txtJg.Text ?? "0"),
                Quantity = int.Parse(txtsl.Text == "" ? "0" : txtsl.Text)
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
            //  _profitItems.Add(newItem);
            _profitItems.Insert(0, newItem);
            button1_Click(null, null);

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
        private void button3_Click(object sender, EventArgs e)
        {

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
     "确认修改",
     MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            var selectedItem = (PriceItem)comboBox1.SelectedItem;
            selectedItem.Price = newPrice;

            // 刷新绑定
            var index = comboBox1.SelectedIndex;
            comboBox1.DataSource = null;
            comboBox1.DataSource = _priceList;
            comboBox1.DisplayMember = "Category";
            comboBox1.ValueMember = "Price";
            comboBox1.SelectedIndex = index;

            // 保存到文件
            PriceManager.SavePrices(_priceList);

            // 刷新现有收益记录中的价格
            foreach (var item in _profitItems.Where(x => x.Category == selectedItem.Category))
            {
                item.Price = newPrice;
            }
            dataGridView1.Refresh();
            button1_Click(null,null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            decimal total = _profitItems.Sum(item => item.ItemProfit);
            label4.Text = $"收 益：{total}W";

            // 计算今日收益
            decimal todayProfit = _profitItems
                .Where(item => item.Time.Date == DateTime.Now.Date)
                .Sum(item => item.ItemProfit);

            // 计算最近一周收益
            decimal lastWeekProfit = _profitItems
                .Where(item => item.Time.Date >= DateTime.Now.Date.AddDays(-7))
                .Sum(item => item.ItemProfit);

            // 计算最近一月收益
            decimal lastMonthProfit = _profitItems
                .Where(item => item.Time.Date >= DateTime.Now.Date.AddDays(-30))
                .Sum(item => item.ItemProfit);

            // 计算最近一年收益
            decimal lastYearProfit = _profitItems
                .Where(item => item.Time.Date >= DateTime.Now.Date.AddDays(-365))
                .Sum(item => item.ItemProfit);
            // 计算最近一月收益
            decimal monthProfit = _profitItems
 .Where(item => item.Category == "点卡" && item.Time.Date.Year == DateTime.Now.Date.Year && item.Time.Date.Month == DateTime.Now.Date.Month)
 .Sum(item => item.ItemProfit);
            // 更新标签
            label6.Text = $"今日：{todayProfit}W";
            label7.Text = $"最近一周：{lastWeekProfit}W";
            label8.Text = $"最近一月：{lastMonthProfit}W";
            label9.Text = $"最近一年：{lastYearProfit}W";
            var categoryCounts = _profitItems
                                .Where(item => item.Category != "点卡" && item.Time.Date.Year == DateTime.Now.Date.Year && item.Time.Date.Month == DateTime.Now.Date.Month)
  .GroupBy(item => item.Category)
  .OrderByDescending(a => a.Key)
  .ToDictionary(g => g.Key, g => g.Count());
            //单价：{txtdk.Text} * 人数* 数量=消耗
            

            var dktotal = Math.Abs(monthProfit);
            var dkxs = dktotal / (6 * decimal.Parse(txtdk.Text)*int.Parse(txtjs.Text));
            lbldk.Text = $"{dkxs:N2} 小时";
            label19.Text = $"{dktotal:N2} W";
            int zl = 0;
            label11.Text = $"";
            label13.Text = $"";
            foreach (var item in categoryCounts)
            {
                zl++;
                if (item.Key.Contains("环"))
                {
                    label13.Text += item.Key + ":" + item.Value + "   ";
                }
                else
                {
                    label11.Text += item.Key + ":" + item.Value + "   ";

                }
            }
            // 刷新数据绑定（假设dataGridView1绑定到_profitItems）
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = _profitItems;

            // 确保有数据时选中第一行
            if (dataGridView1.Rows.Count > 0)
            {
                //  dataGridView1.Rows[0].Selected = true;
                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0]; // 选中第一个单元格
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Time")
            {
                if (e.Value is DateTime dt)
                {
                    e.Value = dt.ToString("MM-dd HH:mm");
                    e.FormattingApplied = true;
                }
            }
        }
        // 限制输入框只能输入数字
        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }
        // 限制输入框只能输入数字
        private void txtSl_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar)) // 允许退格键
            {
                if (!char.IsDigit(e.KeyChar)) // 只允许数字
                {
                    e.Handled = true;
                    return;
                }

                // 如果已经有输入且尝试输入0开头
                if (txtsl.Text.Length == 0 && e.KeyChar == '0')
                {
                    e.Handled = true;
                }
            }
        }

        // 回车键快捷提交
        private void txtQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2.PerformClick();
            }
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空所有数据吗？此操作不可恢复！",
       "确认清空",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Warning) == DialogResult.Yes)
            {


                // 清空数据
                _profitItems.Clear();
                DataManager.ClearData();

                // 刷新界面
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = _profitItems;
                label4.Text = $"收 益：0W";
                button1_Click(null, null);

                MessageBox.Show("数据已清空！");
            }
        }

        private void 设置价格表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var priceEditor = new 价格(_priceList, this);
            priceEditor.ShowDialog();
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void 计时_Click(object sender, EventArgs e)
        {
            if (btnjs.Text == "开始")
            {
                timer1.Start();
                btnjs.Text = "暂停";
            }
            else if (btnjs.Text == "暂停")
            {
                timer1.Stop();
                btnjs.Text = "开始";
            }

        }
        private TimeSpan _elapsedTime = TimeSpan.Zero;  // 累计运行时间

        private void Form1_Load(object sender, EventArgs e)
        {
            // 初始化定时器：每10分钟触发一次
            timer1.Interval = 10 * 60 * 1000; // 600,000毫秒 = 10分钟
            timer1.Tick += Timer1_Tick;
            if (btnjs.Text == "开始")
            {
                timer1.Start();
                lbljs.Text = $"已在线：0";

                btnjs.Text = "暂停";
            }
            else if (btnjs.Text == "暂停")
            {
                timer1.Stop();
                btnjs.Text = "开始";
            }

            // 读取配置
            txtdk.Text = Properties.Settings.Default.DKText;
            txtjs.Text = Properties.Settings.Default.RSText;

        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(600));
                UpdateTimeLabel(_elapsedTime);
                _profitItems.Insert(0, new ProfitItem { Category = "点卡", Price = decimal.Parse(txtdk.Text ?? "0") * decimal.Parse(txtjs.Text), Quantity = 1, Time = DateTime.Now });
                button1_Click(null, null);


            }
            catch (Exception ex)
            {
                MessageBox.Show($"定时任务错误：{ex.Message}");
            }
        }
        private void UpdateTimeLabel(TimeSpan time)
        {
            if (lbljs.InvokeRequired)
            {
                lbljs.Invoke(new Action(() =>
                {
                    lbljs.Text = $"已在线：{time:hh\\:mm\\:ss}";
                }));
            }
            else
            {
                lbljs.Text = $"已在线：{time:hh\\:mm\\:ss}";
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            timer1.Dispose();
        }

        private void txtdk_TextChanged(object sender, EventArgs e)
        {
            // 标记文本已修改，并重启定时器
            _isTextChanged = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveToConfig()
        {
            try
            {
                // 保存到用户配置
                Properties.Settings.Default.DKText = txtdk.Text;
                Properties.Settings.Default.RSText = txtjs.Text;
                Properties.Settings.Default.Save(); // 必须调用Save()方法持久化
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败：{ex.Message}");
            }
        }

        private void txtjs_TextChanged(object sender, EventArgs e)
        {

            // 标记文本已修改，并重启定时器
            _isTextChanged = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }
    }
}
