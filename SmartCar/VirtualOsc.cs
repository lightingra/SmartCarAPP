using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartCar
{
    public partial class VirtualOsc : Form
    {
        public VirtualOsc()
        {
            InitializeComponent();
        }

        int count = 0;

        public void Add(float[] dat)
        {
            chart1.Series[0].Points.AddXY(count, dat[0]);
            chart1.Series[1].Points.AddXY(count, dat[1]);
            chart1.Series[2].Points.AddXY(count, dat[2]);
            chart1.Series[3].Points.AddXY(count, dat[3]);
            chart1.Series[4].Points.AddXY(count, dat[4]);
            chart1.Series[5].Points.AddXY(count, dat[5]);
            count++;

            if(count>100)
            {
                chart1.ChartAreas[0].AxisX.Minimum = count - 100;
                chart1.ChartAreas[0].AxisX.Maximum = count;
            }
        }
    }
}
