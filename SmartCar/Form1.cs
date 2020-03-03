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
                    List<byte> buff = new List<byte>();
                    buff.AddRange(camera.Bytes);
                    int w = (buff[1] << 8) + buff[2];
                    int h = (buff[3] << 8) + buff[4];
                    buff.RemoveRange(0, 7);
                    Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
                    BitmapData dat = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    Marshal.Copy(buff.ToArray(), 0, dat.Scan0, buff.Count);
                    bitmap.UnlockBits(dat);
                    ColorPalette palette = bitmap.Palette;
                    for(int i=0;i<256;i++)
                    {
                        palette.Entries[i] = Color.FromArgb(255,i, i, i);
                    }
                    bitmap.Palette = palette;
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
    }
}
