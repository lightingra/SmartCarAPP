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
using System.Drawing.Imaging;

namespace SmartCar
{
    public partial class Form1 : Form
    {
        ComSerial com = new ComSerial();
        QueueCacheLock<byte[]> textBuff = new QueueCacheLock<byte[]>();
        string text;

        public Form1()
        {
            InitializeComponent();

            com.ComDataReceivedEvent += Com_ComDataReceivedEvent;
        }

        private void Com_ComDataReceivedEvent(byte[] buff)
        {
            textBuff.Add(buff);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开串口")
            {
                button1.Text = com.Connect(comboBox1.Text, comboBox2.Text);
            }
            else
            {
                button1.Text = com.DisConnect();
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(com.Coms);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != null)
            {
                VirtualOsc OscForm = new VirtualOsc(Convert.ToInt32(textBox3.Text));
                OscForm.Text = textBox3.Text;
                com.ComDataReceivedEvent += new ComSerial.ComDataReceived(OscForm.Add);
                OscForm.Show();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Camera cameraForm = new Camera();
            com.ComDataReceivedEvent += new ComSerial.ComDataReceived(cameraForm.Add);
            cameraForm.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            while(textBuff.Count>0)
            {
                text += DateTime.Now.ToString() + Encoding.Default.GetString(textBuff.Read()) + "\r\n";
            }
            if(text!=null)
            {
                textBox1.Text += text;
                text = null;
            }
        }
    }
}
