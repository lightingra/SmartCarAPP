using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using System.Threading;

namespace ClassLibrary
{
    public class ComSerial
    {
        SerialPort port = new SerialPort();

        public ComSerial()
        {
            port.DataReceived += Port_DataReceived;
        }

        #region 数据收发
        Queue<byte[]> buffRx = new Queue<byte[]>();
        ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int len = port.BytesToRead;
            byte[] buff = new byte[len];
            port.Read(buff, 0, len);

            cacheLock.EnterWriteLock();
            try
            {
                buffRx.Enqueue(buff);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public void write(byte[] buff)
        {
            if (IsOpen)
            {
                int len = buff.Length;
                if (len > 0)
                {
                    port.Write(buff, 0, len);
                }
            }
        }

        public int ReadCount
        {
            get
            {
                return buffRx.Count;
            }
        }

        public byte[] Read()
        {
            cacheLock.EnterReadLock();
            try
            {
                if (buffRx.Count > 0)
                {
                    return buffRx.Dequeue();
                }
                return null;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
        #endregion

        #region 串口状态操作
        public string[] Names
        {
            get
            {
                string[] name = SerialPort.GetPortNames();
                int len = name.Length;
                if (len > 0)
                {
                    return name;
                }
                return new string[] { "null" };
            }
        }

        public bool IsOpen
        {
            get
            {
                return port.IsOpen;
            }
        }

        public void Close()
        {
            port.Close();
        }

        public bool Opne(string name)
        {
            try
            {
                if (!port.IsOpen)
                {
                    if (name != null)
                    {
                        port.PortName = name;
                        if (!port.IsOpen)
                        {
                            port.Open();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Opne(string name, string baud)
        {
            try
            {
                if (!port.IsOpen)
                {
                    if (name != null && baud != null)
                    {
                        port.PortName = name;
                        port.BaudRate = Convert.ToInt32(baud);
                        if (!port.IsOpen)
                        {
                            port.Open();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Opne(string name, int baud)
        {
            try
            {
                if (!port.IsOpen)
                {
                    if (name != null)
                    {
                        port.PortName = name;
                        port.BaudRate = baud;
                        if (!port.IsOpen)
                        {
                            port.Open();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }

    public class Camera
    {
        public Camera()
        {
            Thread thread = new Thread(new ThreadStart(Check));
            thread.IsBackground = true;
            thread.Start();
        }

        #region 数据接收
        ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        List<byte> buff = new List<byte>();
        Queue<byte[]> image = new Queue<byte[]>();
        int flag = 0;
        int w, h, len;

        public int BytesCount
        {
            get
            {
                return image.Count;
            }
        }

        public byte[] Bytes
        {
            get
            {
                if(image.Count>0)
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
                    image.Enqueue(buff.GetRange(0, len).ToArray());
                    buff.RemoveRange(0, len);
                    flag = 0;
                }
            }
        }

        #endregion
    }
}
