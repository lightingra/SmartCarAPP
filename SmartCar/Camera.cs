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
using System.Threading;

namespace SmartCar
{
    public partial class Camera : Form
    {
        public Camera()
        {
            InitializeComponent();

            Thread thread = new Thread(new ThreadStart(Check));
            thread.IsBackground = true;
            thread.Start();
        }

        #region 数据接收
        ListCacheLock<byte> buff = new ListCacheLock<byte>();
        QueueCacheLock<Bitmap> image = new QueueCacheLock<Bitmap>();
        int flag = 0;
        int w, h, len;
        Bitmap bitmap;

        /// <summary>
        /// 添加数据到待处理的数据缓存区
        /// </summary>
        /// <param name="bs"></param>
        public void Add(byte[] bs)
        {
            buff.AddRange(bs);
        }

        /// <summary>
        /// 解析协议
        /// </summary>
        private void Check()
        {
            while (true)
            {
                FindHead();
                CheckHead();
                CheckDat();
            }
        }

        /// <summary>
        /// 查找帧头
        /// </summary>
        private void FindHead()
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
        private void CheckHead()
        {
            if (flag == 1)
            {
                if (buff.Count >= 7)
                {
                    if (buff.Read(0) == 0xaa && buff.Read(6) == 0x0d)
                    {
                        w = (buff.Read(1) << 8) + buff.Read(2);
                        h = (buff.Read(3) << 8) + buff.Read(4);
                        len = w * h * buff.Read(5) + 7;
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
        private void CheckDat()
        {
            if (flag == 2)
            {
                if (buff.Count >= len)
                {
                    if (buff.Read(5) == 0x01)
                    {
                        bpp8();
                    }
                    else if (buff.Read(5) == 0x02)
                    {
                        bpprgb565();
                    }
                    else if (buff.Read(5) == 0x03)
                    {
                        bpprgb();
                    }
                    buff.RemoveRange(0, len);
                    flag = 0;
                }
            }
        }
        #endregion

        #region 数据解析
        void bpp8()
        {
            bitmap = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
            BitmapData dat = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            bitmap.Palette = palette;
            Marshal.Copy(buff.GetRange(7, len - 7).ToArray(), 0, dat.Scan0, len - 7);
            bitmap.UnlockBits(dat);
            image.Add(bitmap);
        }

        void bpprgb565()
        {
            bitmap = new Bitmap(w, h, PixelFormat.Format16bppRgb565);
            BitmapData dat = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(buff.GetRange(7, len - 7).ToArray(), 0, dat.Scan0, len - 7);
            bitmap.UnlockBits(dat);
            image.Add(bitmap);
        }

        void bpprgb()
        {
            bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData dat = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(buff.GetRange(7, len - 7).ToArray(), 0, dat.Scan0, len - 7);
            bitmap.UnlockBits(dat);
            image.Add(bitmap);
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(image.Count>0 && checkCamera.Checked)
            {
                pictureBox1.Image = image.Read();
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
                    pictureBox1.Image.Save(saveFile.FileName, ImageFormat.Bmp);
                }
            }
            saveFile.Dispose();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else if (comboBox3.SelectedIndex == 1)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }
    }

    /// <summary>
    /// 队列形式的列表缓存区
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListCacheLock<T>
    {
        List<T> buff = new List<T>();
        ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        /// <summary>
        /// 读取一定范围的数据，深复制
        /// </summary>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public T[] GetRange(int index, int len)
        {
            cacheLock.EnterReadLock();
            try
            {
                return buff.GetRange(index, len).ToArray();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 删除索引处开始一定长度的数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="len"></param>
        public void RemoveRange(int index, int len)
        {
            cacheLock.EnterReadLock();
            try
            {
                buff.RemoveRange(index, len);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
        public T[] Read()
        {
            cacheLock.EnterReadLock();
            try
            {
                return buff.ToArray();
            }
            finally
            {
                cacheLock.EnterReadLock();
            }
        }

        /// <summary>
        /// 从索引处读取数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Read(int index)
        {
            cacheLock.EnterReadLock();
            try
            {
                return buff[index];
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 已缓存的数据大小
        /// </summary>
        public int Count
        {
            get
            {
                return buff.Count;
            }
        }

        /// <summary>
        /// 查找帧头
        /// </summary>
        /// <param name="dat"></param>
        /// <returns></returns>
        public bool IndexOf(T dat)
        {
            cacheLock.EnterReadLock();
            try
            {
                int index = buff.IndexOf(dat);
                if (index != -1)
                {
                    buff.RemoveRange(0, index + 1);
                    return true;
                }
                buff.Clear();
                return false;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 添加数据进入缓存区
        /// </summary>
        /// <param name="dat"></param>
        public void AddRange(T[] dat)
        {
            cacheLock.EnterWriteLock();
            try
            {
                buff.AddRange(dat);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
    }

}
