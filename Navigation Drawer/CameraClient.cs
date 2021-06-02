using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;

using System.Threading;

using System.Diagnostics;

using System.Windows;
using System.Threading;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Linq;

namespace Navigation_Drawer
{
    class FrameCallbackArg : EventArgs 
    {
        public Mat frame;

        public FrameCallbackArg(Mat frame)
        {
            this.frame = frame;
        }
    }

    class EyePointCallbackArg : EventArgs
    {
        public int x, y;

        public EyePointCallbackArg(int x,int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class CameraClient
    {
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public event EventHandler ReceivedFrame;
        public event EventHandler FinishedCalibration;
        public event EventHandler RecvivedEyePoint;
        Thread td_recvData;
        Semaphore sem_recv = new Semaphore(1,1);
        public OpenCvSharp.Point ptEye;

        public CameraClient()
        {
            ptEye.X = -1;
            ptEye.Y = -1;
        }
        public bool isConnected 
        {
            get { return client.Connected; }
        }

        public void Connect(string IP,int port)
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(IP),port));
            td_recvData = new Thread(ThredFunc_recvData);
            td_recvData.IsBackground = true;
            td_recvData.Start();
        }


        public void GetFrame()
        {
            //if (td_RecvingFrame != null && td_RecvingFrame.IsAlive)
            //    return;
            //td_RecvingFrame = new Thread(RecvData);
            //td_RecvingFrame.IsBackground = true;
            //td_RecvingFrame.Start();

            SendCommand("frame");
        }

 

        public void Close() {
            if (this.client.Connected)
                this.client.Close();
        }

        private void SendCommand(string cmd)
        {
            SendString(client, cmd);
        }

        public bool bCal
        {
            set;get;
        }

        void ThredFunc_recvData()
        {
            try
            {
                while (client.Connected)
                {
                    string cmd = RecvString(client);
                    if (cmd == null)
                        break;

                    sem_recv.WaitOne();
                    if (cmd == "frame")
                    {
                        if (ReceivedFrame != null)
                            ReceivedFrame(this, new FrameCallbackArg(RecvImage(client)));
                    }
                    else if(cmd == "Calibration")
                    {
                        int data = RecvInt(client);
                        bCal = (data == 1);

                        if(FinishedCalibration != null)
                            FinishedCalibration(this, null);
                    }
                    else if(cmd == "EyePoint")
                    {
                        this.ptEye.X = RecvInt(client);
                        this.ptEye.Y = RecvInt(client);
                    }

                    sem_recv.Release();
                }
            }
            catch (Exception err)
            {
                Trace.WriteLine(err.Message);
            }
        }
        private void SendInt(Socket sock, int number)
        {
            sock.Send(UTF8Encoding.UTF8.GetBytes(number.ToString() + '\0'));
        }

        private int RecvInt(Socket sock)
        {
            try
            {
                return int.Parse(RecvString(sock));
            }
            catch(FormatException err)
            {
                return 0;
            }
        }

        private String RecvString(Socket sock)
        {
            int RecvSize = 0;
            List<byte> lstData = new List<byte>();

            while (true)
            {
                byte[] data = new byte[1];
                RecvSize = sock.Receive(data);
                if (data[0] == 0)
                    break;
                lstData.Add(data[0]);
            }

            return UTF8Encoding.UTF8.GetString(lstData.ToArray());
        }

        private void SendString(Socket sock,string msg)
        {
            sock.Send(Encoding.UTF8.GetBytes(msg + '\0'));
        }

        private Mat RecvImage(Socket sock)
        {
            List<byte> lstData = new List<byte>();
            int ImgSize = RecvInt(sock);

            if (ImgSize == 0)
                return null;

            while (ImgSize > lstData.Count())
            {
                byte[] bData = new byte[ImgSize - lstData.Count()];

                int RecvSize = sock.Receive(bData);
                Array.Resize(ref bData, RecvSize);
                lstData.AddRange(bData);
            }

            Mat Img = Mat.ImDecode(lstData.ToArray());

            return Img;
        }
    }
}
