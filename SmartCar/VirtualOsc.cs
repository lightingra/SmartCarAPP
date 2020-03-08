using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;

namespace SmartCar
{
    public partial class VirtualOsc : Form
    {
        bool keyFlag = false;

        public VirtualOsc()
        {
            InitializeComponent();
            this.chart1.MouseWheel += Chart1_MouseWheel;
        }

        /// <summary>
        /// 鼠标中间滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (keyFlag)
            {
                if (e.Delta > 0)
                {
                    double maxmin = chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum;
                    chart1.ChartAreas[0].AxisY.Maximum -= maxmin * 0.1;
                    chart1.ChartAreas[0].AxisY.Minimum += maxmin * 0.1;
                }
                else if (e.Delta < 0)
                {
                    double maxmin = chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum;
                    chart1.ChartAreas[0].AxisY.Maximum += maxmin * 0.1;
                    chart1.ChartAreas[0].AxisY.Minimum -= maxmin * 0.1;
                }
            }
            else
            {
                if (e.Delta > 0)
                {
                    double maxmin = chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum;
                    chart1.ChartAreas[0].AxisY.Maximum -= maxmin * 0.1;
                    chart1.ChartAreas[0].AxisY.Minimum -= maxmin * 0.1;
                }
                else if (e.Delta < 0)
                {
                    double maxmin = chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum;
                    chart1.ChartAreas[0].AxisY.Maximum += maxmin * 0.1;
                    chart1.ChartAreas[0].AxisY.Minimum += maxmin * 0.1;
                }
            }
        }

        int count = 0;

        public void Add(float[] dat, string name)
        {
            if (name == this.Text)
            {
                chart1.Series[0].Points.AddXY(count, dat[0]);
                chart1.Series[1].Points.AddXY(count, dat[1]);
                chart1.Series[2].Points.AddXY(count, dat[2]);
                chart1.Series[3].Points.AddXY(count, dat[3]);
                chart1.Series[4].Points.AddXY(count, dat[4]);
                chart1.Series[5].Points.AddXY(count, dat[5]);
                count++;

                if (count > 100)
                {
                    chart1.ChartAreas[0].AxisX.Minimum = count - 100;
                    chart1.ChartAreas[0].AxisX.Maximum = count;
                }
            }
        }

        private void chart1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                keyFlag = true;
            }
        }

        private void chart1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                keyFlag = false;
            }
        }
    }
}
