using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsHW1
{
    public partial class Calculator : Form
    {
        CalculatorModel model = new CalculatorModel();
        public Calculator()
        {
            InitializeComponent();

            digit0.Click += DigitButton_Click;
            digit1.Click += DigitButton_Click;
            digit2.Click += DigitButton_Click;
            digit3.Click += DigitButton_Click;
            digit4.Click += DigitButton_Click;
            digit5.Click += DigitButton_Click;
            digit6.Click += DigitButton_Click;
            digit7.Click += DigitButton_Click;
            digit8.Click += DigitButton_Click;
            digit9.Click += DigitButton_Click;

            adder.Click += OperatorButton_Click;
            subtractor.Click += OperatorButton_Click;
            multiplier.Click += OperatorButton_Click;
            divider.Click += OperatorButton_Click;

            equal.Click += EqualButton_Click;
            clear.Click += ClearButton_Click;
            clearEntry.Click += ClearEntryButton_Click;
            dot.Click += DotButton_Click;

            memoryPlus.Click += MemoryButton_Click;
            memoryMinus.Click += MemoryButton_Click;
            memoryRecall.Click += MemoryButton_Click;
            memoryClean.Click += MemoryButton_Click;
            memoryStore.Click += MemoryButton_Click;
        }

        private void UpdateDisplay()
        {
            answer.Text = model.GetDisplay();
        }

        private void DigitButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int digit = int.Parse(button.Text);
            model.ProcessDigit(digit);
            UpdateDisplay();
        }

        private void DotButton_Click(object sender, EventArgs e)
        {
            model.ProcessDot();
            UpdateDisplay();
        }

        private void OperatorButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Text)
            {
                case "+":
                    model.ProcessPlus();
                    break;
                case "-":
                    model.ProcessMinus();
                    break;
                case "*":
                    model.ProcessMultiply();
                    break;
                case "/":
                    model.ProcessDivide();
                    break;
            }
            UpdateDisplay();
        }

        private void EqualButton_Click(object sender, EventArgs e)
        {
            model.ProcessEqual();
            UpdateDisplay();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            model.Clear();
            UpdateDisplay();
        }

        private void ClearEntryButton_Click(object sender, EventArgs e)
        {
            model.ClearEntry();
            UpdateDisplay();
        }

        private void MemoryButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Text)
            {
                case "M+":
                    model.ProcessMemoryPlus();
                    break;
                case "M-":
                    model.ProcessMemoryMinus();
                    break;
                case "MR":
                    model.ProcessMemoryRecall();
                    break;
                case "MC":
                    model.ProcessMemoryClean();
                    break;
                case "MS":
                    model.ProcessMemoryStore();
                    break;
            }
            UpdateDisplay();
        }
    }
}
