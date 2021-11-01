using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace Numerical5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            richTextBox1.Text += "Function: x^3 + 100 * Sin(x)";
            richTextBox1.Text += Environment.NewLine;
            richTextBox1.Text += Environment.NewLine;

            InitializeMMS();
            DiagonalMMS();

           
            //process rows
            for (int k = 0; k < power + 1; k++)
            {
                for (int i = k + 1; i < power + 1; i++)
                {
                    if (sums[k][k] == 0)
                    {
                       richTextBox1.Text+= "Solution is not exist";
                        return;
                    }
                    double M = sums[i][k] / sums[k][k];
                    for (int j = k; j < power + 1; j++)
                    {
                        sums[i][j] -= M * sums[k][j];
                    }
                    b[i] -= M * b[k];
                }
            }

            //printmatrix();
            for (int i = (power + 1) - 1; i >= 0; i--)
            {
                double s = 0;
                for (int j = i; j < power + 1; j++)
                {
                    s = s + sums[i][j] * a[j];
                }
                a[i] = (b[i] - s) / sums[i][i];
            }

            ResultMMS();

            DrawGraph(zedGraphControl1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private double Function(double x)
        {
            double y;
            y = x * x * x - 3*x*x + 2*x;
            return y;
        }


        public int power = 3;
        public int numofpoints = 5;
        public List<double> a = new List<double>();
        public List<double> b = new List<double>();
        public List<double> x = new List<double>();
        public List<double> y = new List<double>();

        public List<List<double>> sums = new List<List<double>>();

        public double InitializeMMS()
        {
            for (int i = 0; i < power + 1; i++)
            {
                a.Add(0);
                b.Add(0);


                List<double> sumssub = new List<double>();
                for (int j = 0; j < power + 1; j++)
                {
                    
                    sumssub.Add(0);
                }
                sums.Add(sumssub);
            }
            for (int i = 0; i < numofpoints; i++)
            {
                x.Add(i);            
                y.Add(Function(i));              
            }


            //init square sums matrix
            for (int i = 0; i < power + 1; i++)
            {
                for (int j = 0; j < power + 1; j++)
                {
                    sums[i][j] = 0;
                    for (int k = 0; k < numofpoints; k++)
                    {
                        sums[i][j] += Math.Pow(x[k], i + j);
                    }
                }
            }

            //init free coefficients column
            for (int i = 0; i < power + 1; i++)
            {
                for (int j = 0; j < numofpoints; j++)
                {
                    b[i] += Math.Pow(x[j], i) * y[j];
                }
            }

            return 0; 
        }

        //printresults
        public void ResultMMS()
        {
            richTextBox1.Text += "LSM Function: ";
            for (int i = 0; i < power + 1; i++)
            {
                richTextBox1.Text += a[i] + "x^" + i + " + ";
            }

            richTextBox1.Text = richTextBox1.Text.Remove(richTextBox1.Text.Length - 2);
        }

        public double PointResultMMS(double pointX)
        {
            double result = 0;
            for (int i = 0; i < power + 1; i++)
            {
                result += a[i] * Math.Pow(pointX, i);
                
            }

            return result;
        }
        //
        public void DiagonalMMS()
        {
            int i, j, k;
            double temp = 0;
            for (i = 0; i < power + 1; i++)
            {
                if (sums[i][i] == 0)
                {
                    for (j = 0; j < power + 1; j++)
                    {
                        if (j == i) continue;
                        if (sums[j][i] != 0 && sums[i][j] != 0)
                        {
                            for (k = 0; k < power + 1; k++)
                            {
                                temp = sums[j][k];
                                sums[j][k] = sums[i][k];
                                sums[i][k] = temp;
                            }
                            temp = b[j];
                            b[j] = b[i];
                            b[i] = temp;
                            break;
                        }
                    }
                }
            }
        }


        private void DrawGraph(ZedGraphControl zedGraph)
        {


            {
                // Получим панель для рисования
                GraphPane pane = zedGraph.GraphPane;

                // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
                pane.CurveList.Clear();

                // Создадим список точек для кривой f1(x)
                PointPairList f1_list = new PointPairList();

                // Создадим список точек для кривой f2(x)
                PointPairList f2_list = new PointPairList();


                

                double xmin = -10;
                double xmax = 10;

                // !!!
                // Заполним массив точек для кривой f1(x)
                for (double x = xmin; x <= xmax; x += 0.1)
                {
                    f1_list.Add(x, Function(x));
                }

                // !!!
                // Заполним массив точек для кривой f2(x)
                // Интервал и шаги по X могут не совпадать на разных кривых
                for (double x = xmin; x <= xmax; x += 0.1)
                {
                    f2_list.Add(x, PointResultMMS(x));
                }

                
                // !!!
                // которая будет рисоваться голубым цветом (Color.Blue),
                // Опорные точки выделяться не будут (SymbolType.None)

                LineItem f1_curve = pane.AddCurve("Function ", f1_list, Color.Blue, SymbolType.None);

                // !!!
                // Создадим кривую с названием "Sin", 
                // которая будет рисоваться красным цветом (Color.Red),
                // Опорные точки будут выделяться плюсиками (SymbolType.Plus)
                LineItem f2_curve = pane.AddCurve("LSM", f2_list, Color.Red, SymbolType.Plus);

                
                // !!!
                // Устанавливаем интересующий нас интервал по оси X
                pane.XAxis.Min = -2;
                pane.XAxis.Max = 2;

                // !!!
                // Устанавливаем интересующий нас интервал по оси Y

                pane.YAxis.Min = -20;
                pane.YAxis.Max = 20;
                // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
                // В противном случае на рисунке будет показана только часть графика, 
                // которая умещается в интервалы по осям, установленные по умолчанию
                zedGraph.AxisChange();

                // Обновляем график
                zedGraph.Invalidate();
            }
        }
    }
}
