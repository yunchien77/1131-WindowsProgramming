using System.Windows.Forms;
using System.Drawing;

namespace MyDrawing
{
    public class TextEditDialog : Form
    {
        private TextBox _textBox;
        public string TextValue { get; private set; }

        public TextEditDialog(string initialText)
        {
            Text = "文字編輯方塊";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(300, 200);  // 增加整體高度
            FormBorderStyle = FormBorderStyle.FixedDialog;

            // 文字輸入框
            _textBox = new TextBox
            {
                Text = initialText,
                Width = 250,
                Height = 50,  // 增加文字框高度
                TextAlign = HorizontalAlignment.Center,
                Location = new Point((ClientSize.Width - 250) / 2, 40), // 中間對齊並向下移動
                Margin = new Padding(10)
            };
            Controls.Add(_textBox);

            // 按鈕面板，用於居中排列按鈕
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Width = ClientSize.Width,
                Height = 50,
                Padding = new Padding(10),
                Location = new Point((ClientSize.Width - 250) / 2, ClientSize.Height - 70)
            };

            // 確認按鈕
            var okButton = new Button
            {
                Text = "確定",
                DialogResult = DialogResult.OK,
                Width = 100,
                Margin = new Padding(5)
            };

            // 取消按鈕
            var cancelButton = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Margin = new Padding(5)
            };

            // 將按鈕加入按鈕面板並居中排列
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            Controls.Add(buttonPanel);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            TextValue = _textBox.Text;
        }
    }
}
