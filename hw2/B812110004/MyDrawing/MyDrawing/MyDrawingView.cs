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
        private MyDrawingModel model;

        public MyDrawing()
        {
            InitializeComponent();
            model = new MyDrawingModel();
            model.ShapesChanged += Model_ShapesChanged;

            shapeDataGridView.CellContentClick += shapeDataGridView_DeleteShape;
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

        private void Model_ShapesChanged(object sender, EventArgs e)
        {
            RefreshDataGridView();
        }

        private void RefreshDataGridView()
        {
            shapeDataGridView.Rows.Clear();
            foreach (var shape in model.GetShapes())
            {
                shapeDataGridView.Rows.Add(
                    "刪除",
                    shape.Id,
                    shape.GetShapeType(),
                    shape.Text,
                    shape.X,
                    shape.Y,
                    shape.Height,
                    shape.Width
                );
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string shapeType = comboBoxShapeType.SelectedItem?.ToString();
            string text = wordText.Text;
            if (!int.TryParse(XText.Text, out int x) ||
                !int.TryParse(YText.Text, out int y) ||
                !int.TryParse(HText.Text, out int height) ||
                !int.TryParse(WText.Text, out int width) ||
                string.IsNullOrEmpty(shapeType) ||
                string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please fill in all fields with valid values.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (x < 0 || y < 0 || height <= 0 || width <= 0)
            {
                MessageBox.Show("the fields must be positive numbers.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            model.AddShape(shapeType, text, x, y, height, width);
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
        private void shapeDataGridView_DeleteShape(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == shapeDataGridView.Columns["DeleteButton"].Index && e.RowIndex >= 0)
            {
                int id = (int)shapeDataGridView.Rows[e.RowIndex].Cells["IDColumn"].Value;
                model.DeleteShape(id);
            }
        }
    }
}