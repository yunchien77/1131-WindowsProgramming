using MyDrawing.Factories;
using MyDrawing.States;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyDrawing
{
    public partial class MyDrawing : Form
    {
        private MyDrawingModel _model;
        private MyDrawingPresenter _presenter;
        public ToolStripButton CursorIcon => cursorIcon;
        public Panel DrawingPanel => drawingPanel;
        public ToolStrip ShapeToolStrip => shapeToolStrip;

        public MyDrawing()
        {
            InitializeComponent();
            InitializeDrawingPanel();
            InitializeToolStrip();

            _model = new MyDrawingModel();

            // Pass cursor setting action to DrawingState
            _presenter = new MyDrawingPresenter(_model, cursor =>
            {
                drawingPanel.Cursor = cursor;
            });

            // Data bindings
            addButton.DataBindings.Add("Enabled", _presenter, "IsAddButtonEnabled");

            // Subscribe to presenter's property changes
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_presenter.CurrentShapeType))
                {
                    UpdateToolStripButtons();
                }

                if (e.PropertyName == nameof(_presenter.CurrentCursor))
                {
                    drawingPanel.Cursor = _presenter.CurrentCursor;
                }

                if (e.PropertyName == "DrawingPanelRefresh" || e.PropertyName == "SelectedShape")
                {
                    drawingPanel.Refresh();
                }
            };

            shapeDataGridView.CellContentClick += ShapeDataGridView_DeleteShape;
            addButton.Click += AddButton_Click;

            InitializeDataGridView();
            _model.ShapesChanged += Model_ShapesChanged;

            // Setup TextBox validation events
            XText.TextChanged += (s, e) => ValidateInput();
            YText.TextChanged += (s, e) => ValidateInput();
            HText.TextChanged += (s, e) => ValidateInput();
            WText.TextChanged += (s, e) => ValidateInput();
            wordText.TextChanged += (s, e) => ValidateInput();
            comboBoxShapeType.SelectedIndexChanged += (s, e) => ValidateInput();
        }

        private void UpdateToolStripButtons()
        {
            foreach (ToolStripButton item in shapeToolStrip.Items)
            {
                item.Checked = (item.Text == _presenter.CurrentShapeType) ||
                               (item == cursorIcon && _presenter.CurrentShapeType == null);
            }
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
                processIcon,
                cursorIcon
            });

            startIcon.Click += ToolStripButton_Click;
            decisionIcon.Click += ToolStripButton_Click;
            terminatorIcon.Click += ToolStripButton_Click;
            processIcon.Click += ToolStripButton_Click;
            cursorIcon.Click += ToolStripButton_Click;
        }

        private void ToolStripButton_Click(object sender, EventArgs e)
        {
            var button = (ToolStripButton)sender;
            string shapeType = button == cursorIcon ? null : button.Text;

            _presenter.UpdateToolSelection(shapeType, button == cursorIcon);
        }

        private void InitializeDrawingPanel()
        {
            drawingPanel.Paint += DrawingPanel_Paint;
            drawingPanel.MouseDown += DrawingPanel_MouseDown;
            drawingPanel.MouseMove += DrawingPanel_MouseMove;
            drawingPanel.MouseUp += DrawingPanel_MouseUp;
            drawingPanel.MouseEnter += DrawingPanel_MouseEnter;
            drawingPanel.MouseLeave += DrawingPanel_MouseLeave;

            // Use this.SetStyle instead of drawingPanel.SetStyle
            drawingPanel.GetType().GetProperty("DoubleBuffered",
             System.Reflection.BindingFlags.Instance |
             System.Reflection.BindingFlags.NonPublic)
             ?.SetValue(drawingPanel, true, null);
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            var graphics = new GraphicsAdapter(e.Graphics);
            foreach (var shape in _model.GetShapes())
            {
                shape.Draw(graphics);

                // Draw selection rectangle if shape is selected
                if (shape == _presenter.SelectedShape)
                {
                    using (Pen pen = new Pen(Color.Red) { DashStyle = DashStyle.Dash })
                    {
                        e.Graphics.DrawRectangle(pen, shape.PositionX, shape.PositionY, shape.Width, shape.Height);
                    }
                }
            }
            // Draw temporary shape during drawing
            if (_presenter.CurrentState is DrawingState drawingState)
            {
                drawingState.DrawTemporaryShape(e.Graphics);
            }
        }


        private void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _presenter.HandleMouseDown(e.Location);
            }
        }

        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            _presenter.HandleMouseMove(e.Location);
        }

        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _presenter.HandleMouseUp(e.Location);
        }

        private void DrawingPanel_MouseEnter(object sender, EventArgs e)
        {
            drawingPanel.Cursor = _presenter.CurrentCursor;
        }


        private void DrawingPanel_MouseLeave(object sender, EventArgs e)
        {
            drawingPanel.Cursor = Cursors.Default;
        }

        private void ValidateInput()
        {
            _presenter.ValidateInputs(
                comboBoxShapeType.SelectedItem?.ToString(),
                wordText.Text,
                XText.Text,
                YText.Text,
                HText.Text,
                WText.Text
            );

            // Update label colors
            UpdateLabelColor(labelWord, wordText.Text);
            UpdateLabelColor(labelX, XText.Text);
            UpdateLabelColor(labelY, YText.Text);
            UpdateLabelColor(labelH, HText.Text);
            UpdateLabelColor(labelW, WText.Text);
        }

        private void UpdateLabelColor(Label label, string text)
        {
            if (label.Name == "labelWord" || label == labelWord)
            {
                label.ForeColor = string.IsNullOrEmpty(text)
                    ? Color.Red
                    : Color.Black;
            }
            else
            {
                label.ForeColor = string.IsNullOrEmpty(text) ||
                             !int.TryParse(text, out int value) ||
                             value <= 0
                ? Color.Red
                : Color.Black;
            }

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
                MessageBox.Show("The fields must be positive numbers.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _model.AddShape(shapeType, text, positionX, positionY, height, width, _presenter);

            var newShape = _model.GetShapes().LastOrDefault();
            _presenter.SelectedShape = newShape;

            // 重置工具狀態和游標
            _presenter.ResetToolState();

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