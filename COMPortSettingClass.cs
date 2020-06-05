using System;
using System.Collections.Generic;
using System.IO.Ports;
namespace ETRU_TestBench
{
    public class COMPortSettingClass
    {
        #region  Com Set
        public enum BaudRateParam : int
        {
            _300 = 300,
            _600 = 600,
            _1200 = 1200,
            _2400 = 2400,
            _4800 = 4800,
            _9600 = 9600
        }

        public SerialPort serialPort;

        private Queue<byte> _receiveQueue;

        public Queue<byte> ReceiveQueue
        {
            get { return _receiveQueue; }
            set { _receiveQueue = value; }
        }

        public COMPortSettingClass(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits)
        {
            try
            {
                this.ReceiveQueue = new Queue<byte>(1024);

                if (this.serialPort != null)
                {
                    if (this.serialPort.IsOpen)
                    {
                        this.serialPort.Close();
                    }
                }
                else
                {
                    this.serialPort = new SerialPort();
                }

                this.serialPort.PortName = portName;
                this.serialPort.BaudRate = baudRate;
                this.serialPort.Parity = parity;
                this.serialPort.StopBits = stopBits;
                this.serialPort.DataBits = 8;
                this.serialPort.DtrEnable = true;
                this.serialPort.RtsEnable = true;
                this.serialPort.ReceivedBytesThreshold = 1;
                this.serialPort.WriteBufferSize = 1024;
                //this.serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_ReceivedEvent);
                this.serialPort.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void serialPort_ReceivedEvent(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int n = this.serialPort.BytesToRead;
            byte[] buf = new byte[n];
            this.serialPort.Read(buf, 0, n);
            lock (this.ReceiveQueue)
            {
                foreach (byte item in buf)
                {
                    this.ReceiveQueue.Enqueue(item);
                }
            }
        }

        public void DisconnectSerialPort()
        {
            if (this.serialPort != null)
            {
                if (this.serialPort.IsOpen)
                {
                    this.serialPort.Close();
                }
            }
        }

        public void serialPort_WriteData(byte[] buffer)
        {
            this.serialPort.DiscardInBuffer();
            this.serialPort.DiscardOutBuffer();
            lock (this.ReceiveQueue)
            {
                this.ReceiveQueue.Clear();
            }
            this.serialPort.Write(buffer, 0, buffer.Length);
        }
        #endregion

    }
}
