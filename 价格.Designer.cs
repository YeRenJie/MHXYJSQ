namespace mhcj
{
    partial class 价格
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewPrices = new System.Windows.Forms.DataGridView();
            this.类别 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Price = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAddCategory = new System.Windows.Forms.Button();
            this.txtNewCategory = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrices)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewPrices
            // 
            this.dataGridViewPrices.AllowUserToAddRows = false;
            this.dataGridViewPrices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPrices.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.类别,
            this.Price});
            this.dataGridViewPrices.Location = new System.Drawing.Point(40, 39);
            this.dataGridViewPrices.Name = "dataGridViewPrices";
            this.dataGridViewPrices.RowTemplate.Height = 23;
            this.dataGridViewPrices.Size = new System.Drawing.Size(451, 625);
            this.dataGridViewPrices.TabIndex = 4;
            this.dataGridViewPrices.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewPrices_KeyDown_1);
            // 
            // 类别
            // 
            this.类别.HeaderText = "类别";
            this.类别.Name = "类别";
            // 
            // Price
            // 
            this.Price.HeaderText = "价格";
            this.Price.Name = "Price";
            // 
            // btnAddCategory
            // 
            this.btnAddCategory.Location = new System.Drawing.Point(416, 10);
            this.btnAddCategory.Name = "btnAddCategory";
            this.btnAddCategory.Size = new System.Drawing.Size(75, 23);
            this.btnAddCategory.TabIndex = 5;
            this.btnAddCategory.Text = "新增类别";
            this.btnAddCategory.UseVisualStyleBackColor = true;
            this.btnAddCategory.Click += new System.EventHandler(this.btnAddCategory_Click);
            // 
            // txtNewCategory
            // 
            this.txtNewCategory.Location = new System.Drawing.Point(299, 12);
            this.txtNewCategory.Name = "txtNewCategory";
            this.txtNewCategory.Size = new System.Drawing.Size(100, 21);
            this.txtNewCategory.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(228, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "类别名称：";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(40, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 20);
            this.comboBox1.TabIndex = 8;
            // 
            // 价格
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 723);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNewCategory);
            this.Controls.Add(this.btnAddCategory);
            this.Controls.Add(this.dataGridViewPrices);
            this.Name = "价格";
            this.Text = "价格";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.价格_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrices)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewPrices;
        private System.Windows.Forms.DataGridViewTextBoxColumn 类别;
        private System.Windows.Forms.DataGridViewTextBoxColumn Price;
        private System.Windows.Forms.Button btnAddCategory;
        private System.Windows.Forms.TextBox txtNewCategory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}