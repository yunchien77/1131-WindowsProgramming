using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Windows.Forms;
using System;

public class TextEditDialog : Form
{
    private TextBox _textBox;
    private Button _okButton;
    public string TextValue { get; private set; }

    public TextEditDialog(string initialText)
    {
        Text = "文字編輯方塊";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(300, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        TextValue = initialText;  // 初始化 TextValue

        // 文字輸入框
        _textBox = new TextBox
        {
            Text = initialText,
            Width = 250,
            Height = 50,
            TextAlign = HorizontalAlignment.Center,
            Location = new Point((ClientSize.Width - 250) / 2, 40),
            Margin = new Padding(10)
        };
        _textBox.TextChanged += TextBox_TextChanged;
        Controls.Add(_textBox);

        // 按鈕面板
        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Width = ClientSize.Width,
            Height = 50,
            Padding = new Padding(10),
            Location = new Point((ClientSize.Width - 250) / 2, ClientSize.Height - 70)
        };

        // 確認按鈕
        _okButton = new Button
        {
            Text = "確定",
            DialogResult = DialogResult.OK,
            Width = 100,
            Margin = new Padding(5)
        };
        _okButton.Enabled = false; // 初始時禁用

        // 取消按鈕
        var cancelButton = new Button
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel,
            Width = 100,
            Margin = new Padding(5)
        };

        buttonPanel.Controls.Add(_okButton);
        buttonPanel.Controls.Add(cancelButton);
        Controls.Add(buttonPanel);
    }

    private void TextBox_TextChanged(object sender, EventArgs e)
    {
        // 當文字有改變時啟用確認按鈕
        _okButton.Enabled = _textBox.Text != TextValue;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        TextValue = _textBox.Text;
    }

    public void SetTextValue(string value)
    {
        TextValue = value;
    }

}