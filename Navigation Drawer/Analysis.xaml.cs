﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Window = System.Windows.Window;
using Rectangle = System.Drawing.Rectangle;
using System.ComponentModel;
using System.Net.Sockets;
using MessageBox = System.Windows.MessageBox;

namespace Navigation_Drawer
{
    /// <summary>
    /// Analysis.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 


    public partial class Analysis : Page
    {
        Thread th_Analysis;

        AnalysisData data = new AnalysisData()
        {
            bStart = false
        };

        CameraClient cap_Face = new CameraClient();
        CameraClient cap_LeftEye = new CameraClient();
        CameraClient cap_RightEye = new CameraClient();

        Thread td_recvFrame;
        public Analysis()
        {
            InitializeComponent();
            this.DataContext = data;

            try
            {
                cap_Face.Connect("127.0.0.1", 8456);
                cap_LeftEye.Connect("127.0.0.1", 8457);
                cap_RightEye.Connect("127.0.0.1", 8458);
            }
            catch (SocketException err)
            {

            }


            cap_Face.ReceivedFrame += (object obj, EventArgs arg) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    if (arg is FrameCallbackArg)
                    {
                        FrameCallbackArg arg_frame = (arg as FrameCallbackArg);
                        if (arg_frame != null && arg_frame.frame.Empty() == false)
                            img_Face.Source = BitmapSourceConverter.ToBitmapSource((arg as FrameCallbackArg).frame);
                    }
                }));
            };

            cap_LeftEye.ReceivedFrame += (object obj, EventArgs arg) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    if (arg is FrameCallbackArg)
                    {
                        FrameCallbackArg arg_frame = (arg as FrameCallbackArg);
                        if (arg_frame != null && arg_frame.frame.Empty() == false)
                            img_LeftEye.Source = BitmapSourceConverter.ToBitmapSource((arg as FrameCallbackArg).frame);
                    }
                }));
            };

            cap_RightEye.ReceivedFrame += (object obj, EventArgs arg) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    if (arg is FrameCallbackArg)
                    {
                        FrameCallbackArg arg_frame = (arg as FrameCallbackArg);
                        if (arg_frame != null && arg_frame.frame.Empty() == false)
                            img_RightEye.Source = BitmapSourceConverter.ToBitmapSource((arg as FrameCallbackArg).frame);
                    }
                }));
            };


            td_recvFrame = new Thread(ThreadFunc_RecvFrame);
            td_recvFrame.IsBackground = true;
            td_recvFrame.Start();
        }
        private void ThreadFunc_RecvFrame()
        {
            try
            {
                while (cap_Face.isConnected)
                {

                    cap_Face.GetFrame();
                    cap_LeftEye.GetFrame();
                    cap_RightEye.GetFrame();

                    Cv2.WaitKey(30);
                }
            }
            catch (Exception err)
            {

            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            if (th_Analysis != null && th_Analysis.IsAlive)
            {
                return;
            }

            this.th_Analysis = new Thread(ThreadFunc_Analysis);
            th_Analysis.IsBackground = true;
            th_Analysis.Start();
        }

        private ImageSource GetScreenSource(Screen screen)
        {
            Rectangle SecondArea = screen.Bounds;
            Bitmap btMain = new Bitmap(SecondArea.Width, SecondArea.Height);

            using (Graphics g = Graphics.FromImage(btMain))
            {
                g.CopyFromScreen(SecondArea.X, SecondArea.Y, 0, 0, btMain.Size, CopyPixelOperation.SourceCopy);
            }

            return btMain.ToBitmapSource();
        }

        VideoCapture cap;
        private void ThreadFunc_Analysis()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                cap = new VideoCapture("Calibration.mp4");

                MainWindow win_main = (Window.GetWindow(this) as MainWindow);

                if (win_main is null)
                    return;

                SecondWindow win_second = win_main.win_second;
                Screen secondScreen = Screen.AllScreens[1];

                while (cap.IsOpened())
                {
                    Mat frame = new Mat();
                    if (cap.Read(frame))
                    {
                        win_second.img_video.Source = BitmapSourceConverter.ToBitmapSource(frame);
                        //img_SecondScreen.Source = win_second.img_video.Source;
                        img_SecondScreen.Source = GetScreenSource(secondScreen);
                        Cv2.WaitKey(10);
                    }
                    else
                    {
                        break;
                    }
                }

                win_second.img_video.Source = null;
            }));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (cap_Face.isConnected)
                cap_Face.Close();

            if (cap_LeftEye.isConnected)
                cap_LeftEye.Close();

            if (cap_RightEye.isConnected)
                cap_RightEye.Close();

            if (th_Analysis != null && th_Analysis.IsAlive)
            {
                cap.Release();
            }
        }
    }
    public class AnalysisData : INotifyPropertyChanged
    {
        public string name { get; set; }

        int dist;
        public int pupil_distance
        {
            get
            {
                return dist;
            }
            set
            {
                dist = value;
                NotifyPropertyChanged("pupil_distance");
            }
        }

        private bool pbStart = false;
        public bool bStart
        {
            get
            {
                return pbStart;
            }
            set
            {
                pbStart = value;
                NotifyPropertyChanged("bStart");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
