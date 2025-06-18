using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace mhcj
{
    public partial class 价格 : Form
    {
        private Dictionary<string, BindingList<PriceItem>> _serverPrices;
        private string _currentServerId;
        private Form1 _mainForm;

        public 价格(Dictionary<string, BindingList<PriceItem>> serverPrices, string currentServerId, Form1 mainForm)
        {
            InitializeComponent();

            // 保存服务器价格字典和主窗体引用
            _serverPrices = serverPrices;
            _currentServerId = currentServerId;
            _mainForm = mainForm;

            // 初始化服务器下拉框
            InitializeServerComboBox();

            // 配置数据网格
            ConfigureDataGridView();
        }

        private void InitializeServerComboBox()
        {
            // 添加所有服务器
            comboBox1.Items.Clear();
            foreach (var serverId in _serverPrices.Keys)
            {
                comboBox1.Items.Add(serverId);
            }

            // 设置当前服务器
            comboBox1.SelectedItem = _currentServerId;
            comboBox1.SelectedIndexChanged += ComboBoxServers_SelectedIndexChanged;
        }
        private void dataGridViewPrices_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dataGridViewPrices.SelectedRows.Count > 0)
            {
                // 确认删除
                var result = MessageBox.Show("确定要删除选中的类别吗？", "确认删除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // 遍历所有选中的行
                    foreach (DataGridViewRow row in dataGridViewPrices.SelectedRows)
                    {
                        var priceItem = (PriceItem)row.DataBoundItem;
                        _serverPrices[_currentServerId].Remove(priceItem); // 从数据源中删除
                    }
                    MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void ComboBoxServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 切换当前服务器
            _currentServerId = comboBox1.SelectedItem.ToString();

            // 更新数据网格显示
            dataGridViewPrices.DataSource = _serverPrices[_currentServerId];
            dataGridViewPrices.Refresh();
        }

        private void ConfigureDataGridView()
        {
            // 禁用自动生成列
            dataGridViewPrices.AutoGenerateColumns = false;
            dataGridViewPrices.DataSource = _serverPrices[_currentServerId];

            // 添加列
            dataGridViewPrices.Columns.Clear();
            dataGridViewPrices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                DataPropertyName = "Category",
                HeaderText = "类别",
                Width = 150
            });

            dataGridViewPrices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Price",
                DataPropertyName = "Price",
                HeaderText = "价格",
                Width = 100
            });

            // 保存按钮列
            var saveColumn = new DataGridViewButtonColumn
            {
                Name = "SaveColumn",
                HeaderText = "操作",
                Text = "保存",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dataGridViewPrices.Columns.Add(saveColumn);

            // 删除按钮列
            var deleteColumn = new DataGridViewButtonColumn
            {
                Name = "DeleteColumn",
                HeaderText = "操作",
                Text = "删除",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dataGridViewPrices.Columns.Add(deleteColumn);

            // 监听单元格点击事件
            dataGridViewPrices.CellContentClick += DataGridViewPrices_CellContentClick;
        }

        private void DataGridViewPrices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridViewPrices.Rows[e.RowIndex];
                var priceItem = (PriceItem)row.DataBoundItem;
                var columnName = dataGridViewPrices.Columns[e.ColumnIndex].Name;

                if (columnName == "SaveColumn")
                {
                    SavePriceItem(row, priceItem);
                }
                else if (columnName == "DeleteColumn")
                {
                    DeletePriceItem(priceItem);
                }
            }
        }

        private void SavePriceItem(DataGridViewRow row, PriceItem priceItem)
        {
            var priceCell = row.Cells["Price"];
            if (priceCell != null && decimal.TryParse(priceCell.Value?.ToString(), out var newPrice))
            {
                priceItem.Price = newPrice;
                MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("请输入有效的价格！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeletePriceItem(PriceItem priceItem)
        {
            var result = MessageBox.Show("确定要删除该类别吗？", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _serverPrices[_currentServerId].Remove(priceItem);
                MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            var newCategory = txtNewCategory.Text.Trim();
            if (!string.IsNullOrEmpty(newCategory))
            {
                _serverPrices[_currentServerId].Add(new PriceItem { Category = newCategory, Price = 0 });
                dataGridViewPrices.Refresh();
            }
        }

        private void 价格_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存所有服务器的价格
            foreach (var server in _serverPrices)
            {
                PriceManager.SavePrices(server.Value.ToList(), server.Key);
            }

            // 刷新主窗体
            _mainForm.RefreshComboBox();
        }

        private void dataGridViewPrices_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dataGridViewPrices.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show("确定要删除选中的类别吗？", "确认删除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dataGridViewPrices.SelectedRows)
                    {
                        var priceItem = (PriceItem)row.DataBoundItem;
                        _serverPrices[_currentServerId].Remove(priceItem);
                    }
                    MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

       
    }
}