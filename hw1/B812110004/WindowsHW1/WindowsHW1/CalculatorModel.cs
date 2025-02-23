using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace WindowsHW1
{
    internal class CalculatorModel
    {
        private decimal memory;
        private decimal lhs;
        private decimal rhs;
        private string display;
        private string operation;
        private bool isNewCalculation;
        private string lastOperation;
        private decimal? lastRhs;
        private bool isError;

        public CalculatorModel()
        {
            Clear();
        }

        // Digit
        public void ProcessDigit(int digit)
        {
            if (isError)
            {
                Clear();
            }

            if (isNewCalculation)
            {
                display = digit.ToString();
                isNewCalculation = false;
            }
            else
            {
                display += digit.ToString();
            }
        }

        public void ProcessDot()
        {
            if (isError)
            {
                Clear();
            }

            if (!display.Contains("."))
            {
                display += ".";
            }
        }

        // Operation
        public void ProcessPlus()
        {
            if (!isError)
            {
                if (operation != null && isNewCalculation)
                {
                    operation = "+";
                    return;
                }
                PerformOperation();
                operation = "+";
            }
        }

        public void ProcessMinus()
        {
            if (!isError)
            {
                if (operation != null && isNewCalculation)
                {
                    operation = "-";
                    return;
                }
                PerformOperation();
                operation = "-";
            }
        }

        public void ProcessMultiply()
        {
            if (!isError)
            {
                if (operation != null && isNewCalculation)
                {
                    operation = "*";
                    return;
                }
                PerformOperation();
                operation = "*";
            }
        }

        public void ProcessDivide()
        {
            if (!isError)
            {
                if (operation != null && isNewCalculation)
                {
                    operation = "/";
                    return;
                }
                PerformOperation();
                operation = "/";
            }
        }

        public void ProcessEqual()
        {
            if (isError)
            {
                return;
            }

            if (operation != null)
            {
                rhs = decimal.Parse(display);
                PerformOperation();
                lastOperation = operation;
                lastRhs = rhs;
                operation = null;
            }
            else if (lastOperation != null && lastRhs.HasValue)
            {
                rhs = lastRhs.Value;
                operation = lastOperation;
                PerformOperation();
                operation = null;
            }
            isNewCalculation = true;
        }

        private void PerformOperation()
        {
            if (!string.IsNullOrEmpty(operation))
            {
                if (!isNewCalculation)
                {
                    rhs = decimal.Parse(display);
                }
                switch (operation)
                {
                    case "+":
                        lhs += rhs;
                        break;
                    case "-":
                        lhs -= rhs;
                        break;
                    case "*":
                        lhs *= rhs;
                        break;
                    case "/":
                        if (rhs != 0)
                            lhs /= rhs;
                        else
                            SetError();
                        break;
                }
                if (!isError)
                {
                    display = lhs.ToString();
                }
            }
            else
            {
                lhs = decimal.Parse(display);
            }
            isNewCalculation = true;
        }

        private void SetError()
        {
            display = "除數不能為零";
            isError = true;
        }


        // Memory
        public void ProcessMemoryPlus()
        {
            memory += decimal.Parse(display);
            isNewCalculation = true;
        }

        public void ProcessMemoryMinus()
        {
            memory -= decimal.Parse(display);
            isNewCalculation = true;
        }

        public void ProcessMemoryClean()
        {
            memory = 0;
            isNewCalculation = true;
        }

        public void ProcessMemoryStore()
        {
            memory = decimal.Parse(display);
            isNewCalculation = true;
        }

        public void ProcessMemoryRecall()
        {
            display = memory.ToString();
            isNewCalculation = true;
        }

        // Clear
        public void Clear()
        {
            lhs = 0;
            rhs = 0;
            display = "0";
            operation = null;
            lastOperation = null;
            lastRhs = null;
            isNewCalculation = true;
            isError = false;
        }

        public void ClearEntry()
        {
            if (isError)
            {
                Clear();
            }
            else
            {
                display = "0";
                isNewCalculation = true;
            }
        }

        // Display
        public string GetDisplay()
        {
            if (isNewCalculation)
            {
                try
                {
                    decimal number = decimal.Parse(display);
                    if (number % 1 == 0)
                    {
                        display = Math.Round(number).ToString("G29");
                        return display;
                    }
                    else
                    {
                        display = number.ToString("G29");
                        return display;
                    }
                }
                catch
                {
                    return display;
                }
            }
            else
            {
                return display;
            }
        }
    }
}