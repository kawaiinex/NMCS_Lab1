using System.Text;
using StringMath;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace TestGraphs
{
    public partial class Form1 : Form
    {
        double deltaX;
        double epsilon;
        string func;
        string interval;
        double[] values;
        double[] funcValues;
        int numOfSteps;
        PlotView pv;
        List<double> roots;

        public Form1()
        {
            InitializeComponent();
            tableLayoutPanel1.Location = new Point(this.Width / 2, 0);
            tableLayoutPanel1.Width = this.Width / 2 - 20;
            tableLayoutPanel1.Height = this.Height - 100;

            pv = new PlotView();
            pv.InvalidatePlot(true);
            pv.Location = new Point(10, this.Height / 2 - 160);
            pv.Size = new Size(this.Width / 2, this.Height / 2 + 75);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            tableLayoutPanel1.Location = new Point(this.Width / 2, 0);
            tableLayoutPanel1.Width = this.Width / 2 - 20;
            tableLayoutPanel1.Height = this.Height - 100;

            pv.Size = new Size(this.Width / 2, this.Height / 2 + 75);
            pv.Location = new Point(10, this.Height / 2 - 160);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetData();
            GetLimitPoints(out double firstNum, out double secondNum);
            double step = GetStep(firstNum, secondNum);
            List<double[]> intervals = GetIntervals(step, firstNum);
            ResultOutput(firstNum, secondNum, step, intervals);
            DrawGraph();
        }

        void DrawGraph()
        {
            this.Controls.Add(pv);
            pv.Model = new PlotModel();
            pv.Model.PlotType = PlotType.Cartesian;

            LinearAxis XAxis = new LinearAxis
            {
                Position = AxisPosition.Top,
                PositionTier = 0,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                StartPosition = 0,
                EndPosition = 1,
                PositionAtZeroCrossing = true,
            };

            LinearAxis YAxis = new LinearAxis
            {
                Position = AxisPosition.Right,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                PositionTier = 0,
                StartPosition = 0,
                EndPosition = 1,
                PositionAtZeroCrossing = true,
                
            };

            pv.Model.Axes.Add(XAxis);
            pv.Model.Axes.Add(YAxis);


            FunctionSeries fs = new FunctionSeries();
            fs.Color = OxyColors.Red;
            for (int i = 0; i < values.Length; i++)
            {
                fs.Points.Add(new DataPoint(values[i], funcValues[i]));
            }
            pv.Model.Series.Add(fs);

            FunctionSeries rootPoints = new FunctionSeries();
            rootPoints.Color = OxyColors.DarkGray;
            for (int i = 0; i < roots.Count; i++)
            {
                rootPoints.Points.Add(new DataPoint(roots[i], 0));
            }
            rootPoints.LineStyle = LineStyle.None;
            rootPoints.MarkerType = MarkerType.Circle;
            rootPoints.MarkerSize = 5;
            pv.Model.Series.Add(rootPoints);
        }

        void SolvingMethod(List<double[]> intervals)
        {
            roots = new List<double>();
            for (int i = 0; i < intervals.Count; i++)
            {
                double currentValue = intervals[i][0];
                bool first = true;
                double nextValue;
                double iterationNum = 0;
                textBox1.AppendText(Environment.NewLine);
                textBox1.AppendText($"Знайдемо значення x на " +
                    $"проміжку [{intervals[i][0]}; {intervals[i][1]}]" + Environment.NewLine);
                while (true)
                {
                    if (!first)
                    {
                        nextValue = Math.Round(GetNextXValue(currentValue), 4);
                        textBox1.AppendText($"x{iterationNum} = {currentValue} " +
                            $" - f({currentValue}) / f'({currentValue}) = {nextValue}" + Environment.NewLine);
                    }
                    else
                    {
                        nextValue = currentValue;
                        textBox1.AppendText($"x{iterationNum} = {nextValue}" + Environment.NewLine);
                    }
                    double valueOfFunc = Math.Round(GetValueOfFunction(nextValue), 4);
                    textBox1.AppendText($"f({nextValue}) = {valueOfFunc}" + Environment.NewLine);
                    first = false;
                    if (Math.Abs(valueOfFunc) < epsilon)
                    {
                        textBox1.AppendText($"Значення менше за епсілон, " +
                            $"тому {nextValue} є розв'язком." + Environment.NewLine);
                        roots.Add(nextValue);
                        break;
                    }
                    else textBox1.AppendText("Значення більше за епсілон, " +
                        "тому продовжуємо ітерацію." + Environment.NewLine);
                    currentValue = nextValue;
                    iterationNum++;
                }
            }
        }

        void ResultOutput(double firstNum, double secondNum, double step, List<double[]> intervals)
        {
            textBox1.Clear();
            textBox1.AppendText("Значення кроку:" + Environment.NewLine);
            if (firstNum < 0) textBox1.AppendText($"h = |{secondNum} - ({firstNum})| / {numOfSteps} = {step}" + Environment.NewLine);
            else textBox1.AppendText($"h = |{secondNum} - {firstNum}| / {numOfSteps} = {step}" + Environment.NewLine);
            textBox1.AppendText("Таблиця " + Environment.NewLine);
            textBox1.AppendText("x\t\tf(x)" + Environment.NewLine);
            for (int i = 0; i < values.Length; i++)
            {
                textBox1.AppendText($"{values[i]}\t\t{funcValues[i]}" + Environment.NewLine);
            }
            if(intervals.Count > 0)
            {
                textBox1.AppendText("Проміжки на яких знаходяться невідомі значення х:" + Environment.NewLine);
                for (int i = 0; i < intervals.Count; i++)
                {
                    textBox1.AppendText($"[{intervals[i][0]} ; {intervals[i][1]}]" + Environment.NewLine);
                }
            }
            SolvingMethod(intervals);
            textBox1.AppendText("Відповідь: ");
            if(roots.Count == 0) textBox1.AppendText($"коренів не має");
            else
            {
                for (int i = 0; i < roots.Count; i++)
                {
                    if (i == roots.Count - 1) textBox1.AppendText($"x{i + 1} = {roots[i]}.");
                    else textBox1.AppendText($"x{i + 1} = {roots[i]}; ");
                }
            }
        }

        void GetData()
        {
            func = textBox5.Text.ToLower().Replace(',', '.').Replace("pi", "{PI}").Replace("e", "{E}");
            char[] letters = new char[] { 'c', 's', 't' };
            int index = 0;
            for (int i = 0; i < letters.Length; i++)
            {
                index = func.IndexOf(letters[i]);
                if (index > 1)
                {
                    func = func.Insert(index - 1, " ").Insert(index + 1, " ");
                }
            }

            if ((index = func.IndexOf('i')) > 0) FuncCorrect(index);
            else if ((index = func.IndexOf('o')) > 0) FuncCorrect(index);
            else if ((index = func.IndexOf('a')) > 0) FuncCorrect(index);


            interval = textBox2.Text.ToLower().Replace("pi", "{PI}");
            deltaX = Convert.ToDouble(textBox3.Text);
            epsilon = Convert.ToDouble(textBox4.Text);
            numOfSteps = Convert.ToInt32(textBox6.Text);
        }

        void FuncCorrect(int index)
        {
            int number = 0;
            func = func.Substring(0, index - 1) + "(" + func.Substring(index - 1);
            index += 3;
            while (true)
            {
                if (func[index] == '(') number++;
                else if(func[index] == ')')
                {
                    number--;
                    if(number == 0)
                    {
                        func = func.Substring(0, index + 1) + ")" + func.Substring(index + 1);
                        break;
                    }
                }
                index++;
            }
        }

        double GetDerivative(double value) => (GetValueOfFunction(value + deltaX) - GetValueOfFunction(value)) / deltaX;

        double GetValueOfFunction(double value)
        {
            double res = func.Replace("x", $"({value})").Eval();
            if (res == double.NegativeInfinity || res == double.PositiveInfinity) return double.NaN;
            return res;
        }

        double GetNextXValue(double prevValue) => prevValue - (GetValueOfFunction(prevValue) / GetDerivative(prevValue));

        double GetStep(double firstNum, double secondNum) => Math.Abs(firstNum - secondNum) / numOfSteps;

        void GetLimitPoints(out double firstNum, out double secondNum)
        {
            StringBuilder firstNumInSb = new StringBuilder();
            StringBuilder secondNumInSb = new StringBuilder();
            int indexOfComa = interval.IndexOf(',');
            char[] sings = new char[] { '+', '-', '/', '*', '.', '{', '}' };
            for (int i = indexOfComa - 1; i >= 0; i--)
            {
                for (int k = 0; k < sings.Length; k++)
                {
                    if(interval[i] == sings[k]) firstNumInSb.Insert(0, interval[i]);
                }
                if (char.IsDigit(interval[i]) || interval[i] == 'P' || interval[i] == 'I') firstNumInSb.Insert(0, interval[i]);
            }
            for (int i = indexOfComa + 1; i < interval.Length; i++)
            {
                for (int k = 0; k < sings.Length; k++)
                {
                    if (interval[i] == sings[k]) secondNumInSb.Append(interval[i]);
                }
                if (char.IsDigit(interval[i]) || interval[i] == 'P' || interval[i] == 'I') secondNumInSb.Append(interval[i]);
            }
            firstNum = firstNumInSb.ToString().Eval();
            secondNum = secondNumInSb.ToString().Eval();
        }

        List<double[]> GetIntervals(double step, double firstNum)
        {
            values = new double[numOfSteps + 1];
            funcValues = new double[numOfSteps + 1];
            List<double[]> intervals = new List<double[]>();
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Round(firstNum + step * i, 4);
                funcValues[i] = Math.Round(GetValueOfFunction(values[i]), 4);
            }
            for (int i = 0; i < funcValues.Length - 1; i++)
            {
                if ((CheckIfSignChanged(funcValues[i], funcValues[i + 1])) && !funcValues[i].Equals(double.NaN) && !funcValues[i + 1].Equals(double.NaN))
                {
                    if(intervals.Count == 0)
                    {
                        intervals.Add(new double[] { values[i], values[i + 1] });
                    }
                    else if(intervals[intervals.Count - 1][1] != values[i])
                    {
                        intervals.Add(new double[] { values[i], values[i + 1] });
                    }
                }
            }
            return intervals;
        }

        bool CheckIfSignChanged(double val1, double val2)
        {
            if (val1 > 0 && val2 > 0) return false;
            if (val1 < 0 && val2 < 0) return false;
            return true;
        }
    }
}