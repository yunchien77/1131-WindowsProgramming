using MyDrawing.Factories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyDrawing
{
    public partial class MyDrawing : Form
    {
        private MyDrawingModel _model;
        private bool _isDrawing;
        private Point _startPoint;
        private string _currentShapeType;
        private int _tempWidth, _tempHeight;

        public MyDrawing()
        {
            InitializeComponent();
            InitializeDrawingPanel();
            InitializeToolStrip();

            _model = new MyDrawingModel();
            _model.ShapesChanged += Model_ShapesChanged;

            shapeDataGridView.CellContentClick += ShapeDataGridView_DeleteShape;
            addButton.Click += AddButton_Click;

            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            shapeDataGridView.AutoGenerateColumns = false;
            shapeDataGridView.ReadOnly = true;
            shapeDataGridView.AllowUserToAddRows = false;
            RefreshDataGridView();
        }

        private void InitializeToolStrip()
        {
            shapeToolStrip.Items.Clear();
            shapeToolStrip.Items.AddRange(new ToolStripItem[] {
                startIcon,
                decisionIcon,
                terminatorIcon,
                processIcon
            });

            startIcon.Click += ToolStripButton_Click;
            decisionIcon.Click += ToolStripButton_Click;
            terminatorIcon.Click += ToolStripButton_Click;
            processIcon.Click += ToolStripButton_Click;
        }

        private void ToolStripButton_Click(object sender, EventArgs e)
        {
            var button = (ToolStripButton)sender;

            // Uncheck all buttons
            foreach (ToolStripButton item in shapeToolStrip.Items)
            {
                item.Checked = false;
            }

            // Check clicked button
            button.Checked = true;
            _currentShapeType = button.Text;
        }

        private void InitializeDrawingPanel()
        {
            drawingPanel.Paint += DrawingPanel_Paint;
            drawingPanel.MouseDown += DrawingPanel_MouseDown;
            drawingPanel.MouseMove += DrawingPanel_MouseMove;
            drawingPanel.MouseUp += DrawingPanel_MouseUp;
            drawingPanel.MouseEnter += DrawingPanel_MouseEnter;
            drawingPanel.MouseLeave += DrawingPanel_MouseLeave;

            Controls.Add(drawingPanel);
        }


        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            var graphics = new GraphicsAdapter(e.Graphics);
            foreach (var shape in _model.GetShapes())
            {
                shape.Draw(graphics);
            }

            if (_isDrawing && _currentShapeType != null)
            {
                var shape = ShapeFactory.CreateShape(_currentShapeType);
                // 計算繪製位置和大小，確保永遠從起始點向右下方開始
                int drawX = _startPoint.X;
                int drawY = _startPoint.Y;
                int drawWidth = Math.Abs(_tempWidth);
                int drawHeight = Math.Abs(_tempHeight);

                // 根據滑鼠位置調整實際繪製的座標
                if (_tempWidth < 0)
                {
                    drawX = _startPoint.X - drawWidth;
                }
                if (_tempHeight < 0)
                {
                    drawY = _startPoint.Y - drawHeight;
                }

                shape.PositionX = drawX;
                shape.PositionY = drawY;
                shape.Width = drawWidth;
                shape.Height = drawHeight;
                shape.Text = string.Empty; // 拖曳時不顯示文字
                shape.Draw(graphics);
            }
        }

        private void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _currentShapeType != null)
            {
                _isDrawing = true;
                _startPoint = e.Location;
            }
        }

        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _tempWidth = e.X - _startPoint.X;
                _tempHeight = e.Y - _startPoint.Y;
                drawingPanel.Refresh();
            }
        }

        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                int width = Math.Abs(e.X - _startPoint.X);
                int height = Math.Abs(e.Y - _startPoint.Y);

                // 計算最終的繪製位置，考慮反向拖曳的情況
                int finalX = _startPoint.X;
                int finalY = _startPoint.Y;

                if (e.X < _startPoint.X)
                {
                    finalX = _startPoint.X - width;
                }
                if (e.Y < _startPoint.Y)
                {
                    finalY = _startPoint.Y - height;
                }

                // Generate random text
                string randomText = _model.GenerateRandomText();

                _model.AddShape(_currentShapeType, randomText, finalX, finalY, height, width);

                _isDrawing = false;

                // Uncheck all buttons
                foreach (ToolStripButton item in shapeToolStrip.Items)
                {
                    item.Checked = false;
                }
                _currentShapeType = null;

                drawingPanel.Cursor = Cursors.Default;
                drawingPanel.Refresh();
            }
        }

        private void DrawingPanel_MouseEnter(object sender, EventArgs e)
        {
            if (_currentShapeType != null)
            {
                drawingPanel.Cursor = Cursors.Cross;
            }
        }

        private void DrawingPanel_MouseLeave(object sender, EventArgs e)
        {
            drawingPanel.Cursor = Cursors.Default;
        }


        private void Model_ShapesChanged(object sender, EventArgs e)
        {
            RefreshDataGridView();
            drawingPanel.Refresh();
        }

        private void RefreshDataGridView()
        {
            shapeDataGridView.Rows.Clear();
            foreach (var shape in _model.GetShapes())
            {
                shapeDataGridView.Rows.Add(
                    "刪除",
                    shape.Id,
                    shape.GetShapeType(),
                    shape.Text,
                    shape.PositionX,
                    shape.PositionY,
                    shape.Height,
                    shape.Width
                );
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string shapeType = comboBoxShapeType.SelectedItem?.ToString();
            string text = wordText.Text;
            if (!int.TryParse(XText.Text, out int positionX) ||
                !int.TryParse(YText.Text, out int positionY) ||
                !int.TryParse(HText.Text, out int height) ||
                !int.TryParse(WText.Text, out int width) ||
                string.IsNullOrEmpty(shapeType) ||
                string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please fill in all fields with valid values.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (positionX < 0 || positionY < 0 || height <= 0 || width <= 0)
            {
                MessageBox.Show("the fields must be positive numbers.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _model.AddShape(shapeType, text, positionX, positionY, height, width);
            ClearInputFields();
        }

        // clear the textBox
        private void ClearInputFields()
        {
            comboBoxShapeType.SelectedIndex = -1;
            wordText.Clear();
            XText.Clear();
            YText.Clear();
            HText.Clear();
            WText.Clear();
        }

        // delete the shape based on id column
        private void ShapeDataGridView_DeleteShape(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == shapeDataGridView.Columns["DeleteButton"].Index && e.RowIndex >= 0)
            {
                int id = (int)shapeDataGridView.Rows[e.RowIndex].Cells["IDColumn"].Value;
                _model.DeleteShape(id);
                drawingPanel.Refresh();
            }
        }
    }
}