namespace MyDrawing
{
    partial class MyDrawing
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.shapeDataGridView = new System.Windows.Forms.DataGridView();
            this.DeleteButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.IDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShapeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WordColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.XColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.YColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HeightColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WidthColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.說明ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.關於ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxShapeData = new System.Windows.Forms.GroupBox();
            this.dataLabel = new System.Windows.Forms.Label();
            this.WText = new System.Windows.Forms.TextBox();
            this.HText = new System.Windows.Forms.TextBox();
            this.YText = new System.Windows.Forms.TextBox();
            this.XText = new System.Windows.Forms.TextBox();
            this.wordText = new System.Windows.Forms.TextBox();
            this.labelW = new System.Windows.Forms.Label();
            this.labelH = new System.Windows.Forms.Label();
            this.labelY = new System.Windows.Forms.Label();
            this.labelX = new System.Windows.Forms.Label();
            this.comboBoxShapeType = new System.Windows.Forms.ComboBox();
            this.labelWord = new System.Windows.Forms.Label();
            this.addButton = new System.Windows.Forms.Button();
            this.shapeShowgroupBox = new System.Windows.Forms.GroupBox();
            this.Showbutton2 = new System.Windows.Forms.Button();
            this.Showbutton1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.shapeDataGridView)).BeginInit();
            this.menuStrip2.SuspendLayout();
            this.groupBoxShapeData.SuspendLayout();
            this.shapeShowgroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // shapeDataGridView
            // 
            this.shapeDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.shapeDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.shapeDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DeleteButton,
            this.IDColumn,
            this.ShapeColumn,
            this.WordColumn,
            this.XColumn,
            this.YColumn,
            this.HeightColumn,
            this.WidthColumn});
            this.shapeDataGridView.Location = new System.Drawing.Point(5, 95);
            this.shapeDataGridView.Name = "shapeDataGridView";
            this.shapeDataGridView.RowHeadersVisible = false;
            this.shapeDataGridView.RowHeadersWidth = 82;
            this.shapeDataGridView.RowTemplate.Height = 38;
            this.shapeDataGridView.Size = new System.Drawing.Size(700, 1044);
            this.shapeDataGridView.TabIndex = 0;
            // 
            // DeleteButton
            // 
            this.DeleteButton.FillWeight = 185F;
            this.DeleteButton.HeaderText = "刪除";
            this.DeleteButton.MinimumWidth = 10;
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DeleteButton.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // IDColumn
            // 
            this.IDColumn.FillWeight = 85F;
            this.IDColumn.HeaderText = "ID";
            this.IDColumn.MinimumWidth = 10;
            this.IDColumn.Name = "IDColumn";
            // 
            // ShapeColumn
            // 
            this.ShapeColumn.FillWeight = 250F;
            this.ShapeColumn.HeaderText = "形狀";
            this.ShapeColumn.MinimumWidth = 10;
            this.ShapeColumn.Name = "ShapeColumn";
            // 
            // WordColumn
            // 
            this.WordColumn.FillWeight = 250F;
            this.WordColumn.HeaderText = "文字";
            this.WordColumn.MinimumWidth = 10;
            this.WordColumn.Name = "WordColumn";
            // 
            // XColumn
            // 
            this.XColumn.FillWeight = 85F;
            this.XColumn.HeaderText = "X";
            this.XColumn.MinimumWidth = 10;
            this.XColumn.Name = "XColumn";
            // 
            // YColumn
            // 
            this.YColumn.FillWeight = 85F;
            this.YColumn.HeaderText = "Y";
            this.YColumn.MinimumWidth = 10;
            this.YColumn.Name = "YColumn";
            // 
            // HeightColumn
            // 
            this.HeightColumn.FillWeight = 85F;
            this.HeightColumn.HeaderText = "H";
            this.HeightColumn.MinimumWidth = 10;
            this.HeightColumn.Name = "HeightColumn";
            // 
            // WidthColumn
            // 
            this.WidthColumn.FillWeight = 85F;
            this.WidthColumn.HeaderText = "W";
            this.WidthColumn.MinimumWidth = 10;
            this.WidthColumn.Name = "WidthColumn";
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Location = new System.Drawing.Point(0, 38);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(2474, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuStrip2
            // 
            this.menuStrip2.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip2.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.說明ToolStripMenuItem});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(2474, 38);
            this.menuStrip2.TabIndex = 2;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // 說明ToolStripMenuItem
            // 
            this.說明ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.關於ToolStripMenuItem});
            this.說明ToolStripMenuItem.Name = "說明ToolStripMenuItem";
            this.說明ToolStripMenuItem.Size = new System.Drawing.Size(81, 34);
            this.說明ToolStripMenuItem.Text = "說明";
            // 
            // 關於ToolStripMenuItem
            // 
            this.關於ToolStripMenuItem.Name = "關於ToolStripMenuItem";
            this.關於ToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.關於ToolStripMenuItem.Text = "關於";
            // 
            // groupBoxShapeData
            // 
            this.groupBoxShapeData.Controls.Add(this.WText);
            this.groupBoxShapeData.Controls.Add(this.shapeDataGridView);
            this.groupBoxShapeData.Controls.Add(this.wordText);
            this.groupBoxShapeData.Controls.Add(this.labelX);
            this.groupBoxShapeData.Controls.Add(this.XText);
            this.groupBoxShapeData.Controls.Add(this.labelY);
            this.groupBoxShapeData.Controls.Add(this.YText);
            this.groupBoxShapeData.Controls.Add(this.labelH);
            this.groupBoxShapeData.Controls.Add(this.HText);
            this.groupBoxShapeData.Controls.Add(this.labelWord);
            this.groupBoxShapeData.Controls.Add(this.comboBoxShapeType);
            this.groupBoxShapeData.Controls.Add(this.labelW);
            this.groupBoxShapeData.Controls.Add(this.addButton);
            this.groupBoxShapeData.Location = new System.Drawing.Point(1769, 89);
            this.groupBoxShapeData.Name = "groupBoxShapeData";
            this.groupBoxShapeData.Size = new System.Drawing.Size(705, 1139);
            this.groupBoxShapeData.TabIndex = 3;
            this.groupBoxShapeData.TabStop = false;
            // 
            // dataLabel
            // 
            this.dataLabel.AutoSize = true;
            this.dataLabel.Location = new System.Drawing.Point(1775, 89);
            this.dataLabel.Name = "dataLabel";
            this.dataLabel.Size = new System.Drawing.Size(106, 24);
            this.dataLabel.TabIndex = 4;
            this.dataLabel.Text = "資料顯示";
            // 
            // WText
            // 
            this.WText.Location = new System.Drawing.Point(645, 51);
            this.WText.Name = "WText";
            this.WText.Size = new System.Drawing.Size(55, 36);
            this.WText.TabIndex = 13;
            // 
            // HText
            // 
            this.HText.Location = new System.Drawing.Point(582, 51);
            this.HText.Name = "HText";
            this.HText.Size = new System.Drawing.Size(55, 36);
            this.HText.TabIndex = 12;
            // 
            // YText
            // 
            this.YText.Location = new System.Drawing.Point(519, 51);
            this.YText.Name = "YText";
            this.YText.Size = new System.Drawing.Size(55, 36);
            this.YText.TabIndex = 11;
            // 
            // XText
            // 
            this.XText.Location = new System.Drawing.Point(455, 51);
            this.XText.Name = "XText";
            this.XText.Size = new System.Drawing.Size(55, 36);
            this.XText.TabIndex = 10;
            // 
            // wordText
            // 
            this.wordText.Location = new System.Drawing.Point(278, 51);
            this.wordText.Name = "wordText";
            this.wordText.Size = new System.Drawing.Size(159, 36);
            this.wordText.TabIndex = 9;
            // 
            // labelW
            // 
            this.labelW.AutoSize = true;
            this.labelW.Location = new System.Drawing.Point(657, 26);
            this.labelW.Name = "labelW";
            this.labelW.Size = new System.Drawing.Size(31, 24);
            this.labelW.TabIndex = 8;
            this.labelW.Text = "W";
            // 
            // labelH
            // 
            this.labelH.AutoSize = true;
            this.labelH.Location = new System.Drawing.Point(596, 26);
            this.labelH.Name = "labelH";
            this.labelH.Size = new System.Drawing.Size(26, 24);
            this.labelH.TabIndex = 7;
            this.labelH.Text = "H";
            // 
            // labelY
            // 
            this.labelY.AutoSize = true;
            this.labelY.Location = new System.Drawing.Point(533, 26);
            this.labelY.Name = "labelY";
            this.labelY.Size = new System.Drawing.Size(26, 24);
            this.labelY.TabIndex = 6;
            this.labelY.Text = "Y";
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(470, 26);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(26, 24);
            this.labelX.TabIndex = 5;
            this.labelX.Text = "X";
            // 
            // comboBoxShapeType
            // 
            this.comboBoxShapeType.FormattingEnabled = true;
            this.comboBoxShapeType.Items.AddRange(new object[] {
            "Start",
            "Terminator",
            "Process",
            "Decision"});
            this.comboBoxShapeType.Location = new System.Drawing.Point(143, 51);
            this.comboBoxShapeType.Name = "comboBoxShapeType";
            this.comboBoxShapeType.Size = new System.Drawing.Size(121, 32);
            this.comboBoxShapeType.TabIndex = 4;
            this.comboBoxShapeType.Text = "形狀";
            // 
            // labelWord
            // 
            this.labelWord.AutoSize = true;
            this.labelWord.Location = new System.Drawing.Point(325, 26);
            this.labelWord.Name = "labelWord";
            this.labelWord.Size = new System.Drawing.Size(58, 24);
            this.labelWord.TabIndex = 4;
            this.labelWord.Text = "文字";
            // 
            // addButton
            // 
            this.addButton.Font = new System.Drawing.Font("新細明體", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.addButton.Location = new System.Drawing.Point(5, 43);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(124, 44);
            this.addButton.TabIndex = 4;
            this.addButton.Text = "新增";
            this.addButton.UseVisualStyleBackColor = true;
            // 
            // shapeShowgroupBox
            // 
            this.shapeShowgroupBox.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.shapeShowgroupBox.Controls.Add(this.Showbutton2);
            this.shapeShowgroupBox.Controls.Add(this.Showbutton1);
            this.shapeShowgroupBox.Location = new System.Drawing.Point(0, 46);
            this.shapeShowgroupBox.Name = "shapeShowgroupBox";
            this.shapeShowgroupBox.Size = new System.Drawing.Size(227, 1182);
            this.shapeShowgroupBox.TabIndex = 4;
            this.shapeShowgroupBox.TabStop = false;
            // 
            // Showbutton2
            // 
            this.Showbutton2.Location = new System.Drawing.Point(6, 169);
            this.Showbutton2.Name = "Showbutton2";
            this.Showbutton2.Size = new System.Drawing.Size(215, 153);
            this.Showbutton2.TabIndex = 6;
            this.Showbutton2.UseVisualStyleBackColor = true;
            // 
            // Showbutton1
            // 
            this.Showbutton1.Location = new System.Drawing.Point(6, 8);
            this.Showbutton1.Name = "Showbutton1";
            this.Showbutton1.Size = new System.Drawing.Size(215, 153);
            this.Showbutton1.TabIndex = 5;
            this.Showbutton1.UseVisualStyleBackColor = true;
            // 
            // MyDrawing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(2474, 1229);
            this.Controls.Add(this.dataLabel);
            this.Controls.Add(this.shapeShowgroupBox);
            this.Controls.Add(this.groupBoxShapeData);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.menuStrip2);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MyDrawing";
            this.Text = "MyDrawing";
            ((System.ComponentModel.ISupportInitialize)(this.shapeDataGridView)).EndInit();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.groupBoxShapeData.ResumeLayout(false);
            this.groupBoxShapeData.PerformLayout();
            this.shapeShowgroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView shapeDataGridView;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem 說明ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 關於ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxShapeData;
        private System.Windows.Forms.ComboBox comboBoxShapeType;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label labelWord;
        private System.Windows.Forms.TextBox WText;
        private System.Windows.Forms.TextBox HText;
        private System.Windows.Forms.TextBox YText;
        private System.Windows.Forms.TextBox XText;
        private System.Windows.Forms.TextBox wordText;
        private System.Windows.Forms.Label labelW;
        private System.Windows.Forms.Label labelH;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.Label dataLabel;
        private System.Windows.Forms.GroupBox shapeShowgroupBox;
        private System.Windows.Forms.Button Showbutton2;
        private System.Windows.Forms.Button Showbutton1;
        private System.Windows.Forms.DataGridViewButtonColumn DeleteButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn IDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ShapeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn WordColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn XColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn YColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn HeightColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn WidthColumn;
    }
}

