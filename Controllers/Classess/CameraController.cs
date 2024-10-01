using System;
using AForge.Video;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;

namespace ARCA_WPF_F.Controllers.Classess
{
    public class CameraController: INotifyPropertyChanged   
    {
        private string IP;
        private bool IsConnected;
        private MJPEGStream stream;
        private Bitmap FrameData;
        private ImageSource _videoImageSource;
        private DispatcherTimer noUpdateTimer;

        public CameraController() {
            VideoImageSource = GetErrorImageSource();

            noUpdateTimer = new DispatcherTimer();
            noUpdateTimer.Interval = TimeSpan.FromSeconds(3);
            noUpdateTimer.Tick += NoUpdateTimer_Tick;
        }
        public CameraController(string IP)
        {  
            if (String.IsNullOrWhiteSpace(IP)) return;
            this.IP = IP;
        }

        public ImageSource VideoImageSource
        {
            get { return _videoImageSource; }
            set
            {
                _videoImageSource = value;
                OnPropertyChanged("VideoImageSource");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void SetCameraIP(string IP)
        {
            this.IP = IP;
        }
        public void ConnectCamera()
        {
            try
            {
                if (string.IsNullOrEmpty(IP)) return; //throw new InvalidOperationException("IP address cannot be null or empty.");
                stream = new MJPEGStream(IP);
                stream.NewFrame += new NewFrameEventHandler(video_NewFrame);
                stream.Start();
                IsConnected = true;
            }
            catch (Exception)
            {
                VideoImageSource = GetVideoImageSource();
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            FrameData = new Bitmap(eventArgs.Frame);
            VideoImageSource = GetVideoImageSource();

            noUpdateTimer.Stop();
            noUpdateTimer.Start();
        }

        private void NoUpdateTimer_Tick(object sender, EventArgs e)
        {
            VideoImageSource = GetErrorImageSource();
        }

        public Bitmap GetErrorImage()
        {
            return new Bitmap("CameraError.png");
        }

        public ImageSource GetErrorImageSource() {
            return new BitmapImage(new Uri("CameraError.png", UriKind.Relative));
        }

        public Bitmap GetVideo()
        {
            if (IsConnected) return FrameData;
            else return GetErrorImage();
        }

        public ImageSource GetVideoImageSource()
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = GetVideo())
                {
                    bi = bitmap.ToBitmapImage();
                }
                if (bi == null) {
                    if (FrameData == null) return GetErrorImageSource();
                        else return FrameData.ToBitmapImage();
                }
                bi.Freeze();
                return bi;
            }
            catch (Exception)
            {
                return GetErrorImageSource();
            }
        }

        public void DisconnectCamera()
        {
            if(!IsConnected) return;
            IsConnected = false;
            stream.Stop();
        }

        public bool CheckCameraStatus()
        {
            if (IsConnected) { return true; }
            else return false;
        }

        public string GetCameraIP()
        {
            if (IsConnected) { return IP; } else return null;
        }

    }
}

