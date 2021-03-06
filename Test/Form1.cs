﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.VideoStab;
using Emgu.CV.Structure;
using System.Threading;

using SmartCar;

namespace Test
{
    public partial class Form1 : Form
    {
        ComSerial com = new ComSerial();
        VideoCapture cameras = new VideoCapture(1);
        Mat mat = new Mat();
        Mat mat1 = new Mat();
        bool flag = false;

        public Form1()
        {
            InitializeComponent();

            cameras.ImageGrabbed += Cameras_ImageGrabbed;
            cameras.Start();
        }

        private void Cameras_ImageGrabbed(object sender, EventArgs e)
        {
            cameras.Retrieve(mat, 0);
            imageBox1.Image = mat;
            CvInvoke.Resize(mat, mat1, new Size(80, 60));
            byte[] head = new byte[] { 0x55, 0xaa, (byte)(mat1.Width >> 8), (byte)(mat1.Width & 0xff), (byte)(mat1.Height >> 8), (byte)(mat1.Height & 0xff), 0x03, 0xad };
            com.Send(head);
            byte[] img = mat1.ToImage<Rgb, byte>().Bytes;
            com.Send(img);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (com.IsOpen)
            {
                button1.Text = "关闭串口";
            }
            else
            {
                button1.Text = "打开串口";
            }
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
            flag = !flag;
        }
    }
}
