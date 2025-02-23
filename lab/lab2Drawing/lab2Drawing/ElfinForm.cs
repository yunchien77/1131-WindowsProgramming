using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace lab2Drawing
{
    class ElfinForm : Form
    {

        protected int WINDOW_WIDTH = 400;
        protected int WINDOW_HEIGHT = 400;
        protected int BALL_SIZE = 50;

        public ElfinForm()
        {
            SetClientSizeCore(WINDOW_WIDTH, WINDOW_HEIGHT);
            BackColor = Color.Black;
            Text = "Elfin";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // 建立粗筆刷
            Pen thickPen = new Pen(Color.Purple, 7.0f);

            // 繪製三個藍色方塊
            g.FillRectangle(Brushes.Blue, 0, 0, 150, 100);
            g.FillRectangle(Brushes.Blue, 0, 250, 150, 150);
            g.FillRectangle(Brushes.Blue, 300, 0, 100, 275);

            // 繪製小精靈(Elfin)
            g.FillPie(Brushes.Yellow, 15, 115, 120, 120, 30.0f, 300.0f);
            g.FillEllipse(Brushes.Black, 75, 130, 20, 20);

            // 繪製五個綠黃色球
            g.FillEllipse(Brushes.GreenYellow, 200, 25, BALL_SIZE, BALL_SIZE);
            g.FillEllipse(Brushes.GreenYellow, 200, 125, BALL_SIZE, BALL_SIZE);
            g.FillEllipse(Brushes.GreenYellow, 200, 225, BALL_SIZE, BALL_SIZE);
            g.FillEllipse(Brushes.GreenYellow, 200, 325, BALL_SIZE, BALL_SIZE);
            g.FillEllipse(Brushes.GreenYellow, 300, 325, BALL_SIZE, BALL_SIZE);

            // 繪製球的外框
            g.DrawEllipse(thickPen, 200, 25, 50, 50);
            g.DrawEllipse(thickPen, 200, 125, 50, 50);
            g.DrawEllipse(thickPen, 200, 225, 50, 50);
            g.DrawEllipse(thickPen, 200, 325, 50, 50);
            g.DrawEllipse(thickPen, 300, 325, 50, 50);

            // 更改筆刷顏色為粉紅色
            thickPen.Color = Color.Pink;

            // 繪製牆壁的外框
            g.DrawRectangle(thickPen, 0, 0, 150, 100);
            g.DrawRectangle(thickPen, 0, 250, 150, 150);
            g.DrawRectangle(thickPen, 300, 0, 100, 275);
        }
    }
}
