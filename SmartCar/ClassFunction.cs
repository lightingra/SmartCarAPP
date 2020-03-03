using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

using System.Threading;

namespace SmartCar
{
    public class ComCamera
    {
        public ComCamera()
        {
            Thread thread = new Thread(new ThreadStart(Check));
            thread.IsBackground = true;
            thread.Start();
        }

        #region 数据接收
        ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        List<byte> buff = new List<byte>();
        Queue<Bitmap> image = new Queue<Bitmap>();
        int flag = 0;
        int w, h, len;
        Bitmap bitmap;

        public int BytesCount
        {
            get
            {
                return image.Count;
            }
        }

        public Bitmap Bytes
        {
            get
            {
                if (image.Count > 0)
                {
                    cacheLock.EnterReadLock();
                    try
                    {
                        return image.Dequeue();
                    }
                    finally
                    {
                        cacheLock.ExitReadLock();
                    }
                }
                return null;
            }
        }

        public void Add(byte[] bs)
        {
            cacheLock.EnterWriteLock();
            try
            {
                buff.AddRange(bs);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public void Check()
        {
            while (true)
            {
                cacheLock.EnterReadLock();
                try
                {
                    FindHead();
                    CheckHead();
                    CheckDat();
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }
            }
        }

        void FindHead()
        {
            if (flag == 0)
            {
                int index = buff.IndexOf(0x55);
                if (index != -1)
                {
                    buff.RemoveRange(0, index + 1);
                    flag = 1;
                }
                else
                {
                    buff.Clear();
                }
            }
        }

        void CheckHead()
        {
            if (flag == 1)
            {
                if (buff.Count >= 7)
                {
                    if (buff[0] == 0xaa && buff[6] == 0xaa + buff[5])
                    {
                        w = (buff[1] << 8) + buff[2];
                        h = (buff[3] << 8) + buff[4];
                        len = w * h * buff[5] + 7;
                        flag = 2;
                        return;
                    }
                    flag = 0;
                }
            }
        }

        void CheckDat()
        {
            if (flag == 2)
            {
                if (buff.Count >= len)
                {
                    if (buff[5] == 0x01)
                    {
                        bpp8();
                    }
                    else if (buff[5] == 0x02)
                    {
                        bpprgb565();
                    }
                    else if (buff[5] == 0x03)
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
            image.Enqueue(bitmap);
        }

        void bpprgb565()
        {
            bitmap = new Bitmap(w, h, PixelFormat.Format16bppRgb565);
            BitmapData dat = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(buff.GetRange(7, len - 7).ToArray(), 0, dat.Scan0, len - 7);
            bitmap.UnlockBits(dat);
            image.Enqueue(bitmap);
        }

        void bpprgb()
        {
            bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData dat = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(buff.GetRange(7, len - 7).ToArray(), 0, dat.Scan0, len - 7);
            bitmap.UnlockBits(dat);
            image.Enqueue(bitmap);
        }
        #endregion
    }


}
