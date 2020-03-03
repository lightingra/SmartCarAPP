using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using ClassLibrary;
using System.Drawing.Imaging;

namespace SmartCar
{
    public partial class Form1 : Form
    {
        ComSerial com = new ComSerial();
        ComCamera camera = new ComCamera();
        Bitmap bitmap;

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

            if (tabControl1.SelectedIndex == 0) //文本
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
                if(camera.BytesCount>0)
                {
                    bitmap = new Bitmap(camera.Bytes);
                    pictureBox1.Image = bitmap;
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

        private void button5_Click(object sender, EventArgs e)
        {
            if(textBox2.Text!=null)
            {
                com.write(Encoding.Default.GetBytes(textBox2.Text));
                textBox2.Clear();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = ".bmp";
            saveFile.Filter = "位图(*.bmp)|*.bmp";
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Save(saveFile.FileName,ImageFormat.Bmp);
                }
            }
            saveFile.Dispose();
        }
    }
}
