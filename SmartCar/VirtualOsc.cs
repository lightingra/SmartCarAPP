using System.Windows.Forms;
using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Design;
using System.Collections.Generic;
using System.Linq;

namespace SmartCar
{
    public partial class VirtualOsc : Form
    {
        public VirtualOsc(int id)
        {
            InitializeComponent();

            ID = id;

            this.Name = "虚拟示波器 ID: " + ID.ToString();
            this.chart1.MouseWheel += Chart1_MouseWheel;

            Thread thread = new Thread(new ThreadStart(Check));
            thread.IsBackground = true;
            thread.Start();
        }

        #region 数据解析
        /// <summary>
        /// 变量
        /// </summary>
        ListCacheLock<byte> buff = new ListCacheLock<byte>();
        QueueCacheLock<float[]> cache = new QueueCacheLock<float[]>();
        int flag = 0;
        int len = 0;

        /// <summary>
        /// 解析协议
        /// </summary>
        void Check()
        {
            while (true)
            {
                FindHead();
                CheckHead();
                CheckDat();
            }
        }

        /// <summary>
        /// 添加数据到待处理的数据缓存区
        /// </summary>
        /// <param name="bs"></param>
        public void Add(byte[] bs)
        {
            buff.AddRange(bs);
        }

        /// <summary>
        /// 查找帧头
        /// </summary>
        void FindHead()
        {
            if (flag == 0)
            {
                if (buff.IndexOf(0x55))
                {
                    flag = 1;
                }
            }
        }

        /// <summary>
        /// 校核帧头
        /// </summary>
        void CheckHead()
        {
            if (flag == 1)
            {
                if (buff.Count >= 3)
                {
                    if (buff.Read(0) == ID && buff.Read(2) == 0x0d && buff.Read(1) < 7)
                    {
                        len = buff.Read(1) * 4 + 3;
                        flag = 2;
                        return;
                    }
                    flag = 0;
                }
            }
        }

        /// <summary>
        /// 校核数据
        /// </summary>
        void CheckDat()
        {
            if (flag == 2)
            {
                if (buff.Count >= len)
                {
                    float[] datBuff = new float[6];
                    for (int i = 0; i < buff.Read(1); i++)
                    {
                        datBuff[i] += buff.Read(3 + i * 4) << 24;
                        datBuff[i] += buff.Read(4 + i * 4) << 16;
                        datBuff[i] += buff.Read(5 + i * 4) << 8;
                        datBuff[i] += buff.Read(6 + i * 4) << 0;
                    }
                    cache.Add(datBuff);
                    buff.RemoveRange(0, len);
                    flag = 0;
                }
            }
        }
        #endregion

        #region 添加数据

        int count = 0;
        int ID = 0;

        void AddPoint(float[] dat)
        {
            chart1.Series[0].Points.AddXY(count, dat[0]);
            chart1.Series[1].Points.AddXY(count, dat[1]);
            chart1.Series[2].Points.AddXY(count, dat[2]);
            chart1.Series[3].Points.AddXY(count, dat[3]);
            chart1.Series[4].Points.AddXY(count, dat[4]);
            chart1.Series[5].Points.AddXY(count, dat[5]);
            count++;

            this.textBox1.Text = dat[0].ToString();
            this.textBox2.Text = dat[1].ToString();
            this.textBox3.Text = dat[2].ToString();
            this.textBox4.Text = dat[3].ToString();
            this.textBox5.Text = dat[4].ToString();
            this.textBox6.Text = dat[5].ToString();

            if (count > 100)
            {
                chart1.ChartAreas[0].AxisX.Minimum = count - 100;
                chart1.ChartAreas[0].AxisX.Maximum = count;
            }
        }
        #endregion

        bool keyFlag = false;

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

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(e.Location, true);
            chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(e.Location, true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cache.Count > 0)
            {
                AddPoint(cache.Read());

                this.textBox1.BackColor = chart1.Series[0].Color;
                this.textBox2.BackColor = chart1.Series[1].Color;
                this.textBox3.BackColor = chart1.Series[2].Color;
                this.textBox4.BackColor = chart1.Series[3].Color;
                this.textBox5.BackColor = chart1.Series[4].Color;
                this.textBox6.BackColor = chart1.Series[5].Color;
            }
        }

        private void VirtualOsc_Load(object sender, EventArgs e)
        {

        }
    }
}
