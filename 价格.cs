using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace mhcj
{
    public partial class 价格 : Form
    {
        private BindingList<PriceItem> _priceList;
        private Form1 form1; // 保存 Form1 的引用

        public 价格(BindingList<PriceItem> priceList, Form1 form1)
        {
            InitializeComponent();
            _priceList = priceList;
            dataGridViewPrices.DataSource = _priceList;
            this.form1 = form1; // 初始化 Form1 的引用

            ConfigureDataGridView();

        }
        private void ConfigureDataGridView()
        {
            // 禁用自动生成列
            dataGridViewPrices.AutoGenerateColumns = false;

            // 添加列
            dataGridViewPrices.Columns.Clear();
            dataGridViewPrices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category", // 显式设置列的Name属性
                DataPropertyName = "Category",
                HeaderText = "类别",
                Width = 150
            });

            dataGridViewPrices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Price", // 显式设置列的Name属性
                DataPropertyName = "Price",
                HeaderText = "价格",
                Width = 100
            });

            // 保存按钮列
            var saveColumn = new DataGridViewButtonColumn
            {
                Name = "SaveColumn", // 设置列的唯一名称
                HeaderText = "操作",
                Text = "保存",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dataGridViewPrices.Columns.Add(saveColumn);
            // 添加删除按钮列
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

                // 确保点击的是按钮列且行索引有效
                if ( dataGridViewPrices.Columns[e.ColumnIndex].Name == "SaveColumn")
                {
                    // 获取价格列的值
                    var priceCell = row.Cells["Price"]; // 确保列名与 DataPropertyName 一致
                    if (priceCell != null && decimal.TryParse(priceCell.Value?.ToString(), out var newPrice))
                    {
                        priceItem.Price = newPrice;
                        MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("请输入有效的价格！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } // 检查是否点击了删除按钮列
                else if (dataGridViewPrices.Columns[e.ColumnIndex].Name == "DeleteColumn")
                {
                    // 确认删除
                    var result = MessageBox.Show("确定要删除该类别吗？", "确认删除",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        _priceList.Remove(priceItem); // 从数据源中删除
                        MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            var newCategory = txtNewCategory.Text.Trim();
            if (!string.IsNullOrEmpty(newCategory))
            {
                _priceList.Add(new PriceItem { Category = newCategory, Price = 0 });
                dataGridViewPrices.Refresh();
            }
        }

        private void 价格_FormClosing(object sender, FormClosingEventArgs e)
        {
            PriceManager.SavePrices(_priceList);
            form1.RefreshComboBox();
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
                        _priceList.Remove(priceItem); // 从数据源中删除
                    }
                    MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
