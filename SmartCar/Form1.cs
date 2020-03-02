using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ClassLibrary;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SmartCar
{
    public partial class Form1 : Form
    {
        ComSerial com = new ComSerial();
        Camera camera = new Camera();
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(com.IsOpen)
            {
                button1.Text = "关闭串口";
            }
            else
            {
                button1.Text = "打开串口";
            }

            if (tabControl1.SelectedIndex == 0)
            {
                if (com.ReadCount > 0)
                {
                    if (radioButton1.Checked)
                    {
                        textBox1.Text += DateTime.Now.ToString("hh:mm:ss") + ":" + Encoding.Default.GetString(com.Read()) + "\r\n";
                    }
                    else
                    {
                        textBox1.Text += DateTime.Now.ToString("hh:mm:ss") + ":" + BitConverter.ToString(com.Read()) + "\r\n";
                    }
                }
            }
            else if(tabControl1.SelectedIndex == 1)
            {
                if(com.ReadCount>0)
                {
                    camera.Add(com.Read());
                }
                if(camera.image.Count>0)
                {
                    pictureBox1.Image = camera.image.Dequeue().ToBitmap();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "打开串口")
            {
                com.Opne(comboBox1.Text, comboBox2.Text);
            }
            else
            {
                com.Close();
            }  
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(com.Names);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
    }
}
