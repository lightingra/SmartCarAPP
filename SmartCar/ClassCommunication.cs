using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using System.Threading;

namespace SmartCar
{
    /// <summary>
    /// 队列缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueCacheLock<T>
    {
        Queue<T> buff = new Queue<T>();
        ReaderWriterLockSlim cahceLock = new ReaderWriterLockSlim();

        public void Add(T dat)
        {
            cahceLock.EnterWriteLock();
            try
            {
                buff.Enqueue(dat);
            }
            finally
            {
                cahceLock.ExitWriteLock();
            }
        }

        public T Read()
        {
            cahceLock.EnterReadLock();
            try
            {
                return buff.Dequeue();
            }
            finally
            {
                cahceLock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                return buff.Count;
            }
        }
    }

    /// <summary>
    /// 串口通信
    /// </summary>
    public class ComSerial
    {
        SerialPort com = new SerialPort();

        public delegate void ComDataReceived(byte[] buff);
        public event ComDataReceived ComDataReceivedEvent;

        public ComSerial()
        {
            com.DataReceived += Com_DataReceived;
            ComDataReceivedEvent += ComSerial_ComDataReceivedEvent;
        }

        private void ComSerial_ComDataReceivedEvent(byte[] buff)
        {
            
        }

        //将收到的数据加入队列缓冲区
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int len = com.BytesToRead;
            byte[] buff = new byte[len];
            com.Read(buff, 0, len);
            ComDataReceivedEvent(buff);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buff"></param>
        public void Send(byte[] buff)
        {
            if(buff.Length>0 && com.IsOpen)
            {
                com.Write(buff, 0, buff.Length);
            }
        }

        /// <summary>
        /// 获取可用的串口
        /// </summary>
        public string[] Coms
        {
            get
            {
                string[] coms = SerialPort.GetPortNames();
                if(coms.Length!=0)
                {
                    return coms;
                }
                return new string[] { "null"};
            }
        }

        /// <summary>
        /// 串口当前状态
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return com.IsOpen;
            }
        }

        /// <summary>
        /// 断开串口连接
        /// </summary>
        /// <returns></returns>
        public string DisConnect()
        {
            com.Close();
            return "打开串口";
        }

        /// <summary>
        /// 建立串口连接
        /// </summary>
        /// <param name="name"></param>
        /// <param name="baud"></param>
        /// <returns></returns>
        public string Connect(string name ,string baud)
        {
            try
            {
                if(!com.IsOpen)
                {
                    if(name!=null && baud!=null)
                    {
                        com.PortName = name;
                        com.BaudRate = Convert.ToInt32(baud);
                        if(!com.IsOpen)
                        {
                            com.Open();
                            return "关闭串口";
                        }
                    }
                }
                return "打开串口";
            }
            catch
            {
                return "打开串口";
            }
        }
    }
}
