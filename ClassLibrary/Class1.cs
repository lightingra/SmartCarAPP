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
}
