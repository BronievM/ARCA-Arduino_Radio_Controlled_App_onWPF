using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ARCA_WPF_F.Controllers;
using ARCA_WPF_F.Controllers.Classess.Arduino;
using ARCA_WPF_F.Resources;

namespace ARCA_WPF_F
{
    public partial class MainWindow : Window
    {
        private MainController mc;
        private SettingsWindow settingsWindow;
        private string ProgName = "ARCA";
        public MainWindow()
        {
            InitializeComponent();
            mc = new MainController();
            mc.DataSent += MainController_DataSent;
            mc.DataUpdated += MainController_DataUpdated;
            DebugMenuItem.IsChecked = mc.GetDebug();
            DebugPanel.Visibility = mc.GetDebug() ? Visibility.Visible : Visibility.Hidden;
            MainWind.Title = ProgName;
            mc.ConnectCamera();
            Connect();
            UIConnectedControllerEvent();

        }

        private void UIConnectedControllerEvent()
        {
            if(mc != null )
            {
                if(mc.IsControllerConnected()) {
                    Gamepad_Disc_Label.Visibility = Visibility.Collapsed;
                    DataDebugLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    Gamepad_Disc_Label.Visibility = Visibility.Visible;
                    DataDebugLabel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MainController_DataSent(DateTime time, bool IsNRFConnected, bool IsDataReceived)
        {
            this.Dispatcher.Invoke(() =>
            {
                UIConnectedControllerEvent();
                DateTime dtNow = DateTime.Now;
                double elapsedMilliseconds = (dtNow - time).TotalMilliseconds;
                TimeSendText.Text = $"{time.ToLongTimeString()}";

                if (IsDataReceived)
                {
                    DataStatusText.Text = "Sended succesfully!";
                    DataStatusText.Foreground = Brushes.Green;
                }
                else if(!IsDataReceived && !IsNRFConnected) 
                {
                    DataStatusText.Text = "There is a problem with radiomodule";
                    DataStatusText.Foreground = Brushes.IndianRed;
                }
                else if (!IsDataReceived)
                {
                    DataStatusText.Text = "Data is sended, but maybe not received";
                    DataStatusText.Foreground = Brushes.IndianRed;
                }

                if (mc.CheckArduinoStatus())
                {
                    ArduinoStatusText.Text = "connected!";
                    ArduinoStatusText.Foreground = Brushes.Green;
                }
                else
                {
                    ArduinoStatusText.Text = "disconnected!";
                    ArduinoStatusText.Foreground = Brushes.IndianRed;
                }

                MainWind.Title = ProgName + " => " + DataStatusText.Text;
            });
        }

        private void MainController_DataUpdated(DataStruct data)
        {
            this.Dispatcher.Invoke(() =>
            {
                DataStruct datas = data;

                string steer = datas.steer.ToString();
                string speed = (Math.Abs(datas.accelerate - datas.brake)).ToString();  
                
                SteerLabel.Text = steer;
                SpeedLabel.Text = speed;
                F1Label.Text = datas.F1? "1" : "0";
                F1Label.Foreground = datas.F1 ? Brushes.Green : Brushes.Red;
                F2Label.Text = datas.F2? "1" : "0";
                F2Label.Foreground = datas.F2 ? Brushes.Green : Brushes.Red;
            });
        }


        private void Connect()
        {
            Binding binding = new Binding("Camera.VideoImageSource");
            binding.Source = mc;
            CameraImage.SetBinding(System.Windows.Controls.Image.SourceProperty, binding);
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            mc.DisconnectCamera();
            mc.DisconnectArduino();
            mc.Saving();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow = new SettingsWindow(mc);
            settingsWindow.Closed += SettingsWindow_Closed;
            settingsWindow.ShowDialog();
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            mc.DisconnectCamera();
            mc = settingsWindow.GetMainController();
            mc.ConnectCamera();
            Connect();
            UIConnectedControllerEvent();

            if (mc.CheckArduinoStatus()) {
                ArduinoStatusText.Text = "connected!";
                ArduinoStatusText.Foreground = Brushes.Green;
            }
            else
            {
                ArduinoStatusText.Text = "disconnected!";
                ArduinoStatusText.Foreground = Brushes.IndianRed;
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutBorder.Visibility = Visibility.Visible;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            AboutBorder.Visibility = Visibility.Hidden;
        }

        private void DebugMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            DebugPanel.Visibility = Visibility.Visible;
            mc.SetDebug(DebugMenuItem.IsChecked);
        }

        private void DebugMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            DebugPanel.Visibility = Visibility.Collapsed;
            mc.SetDebug(DebugMenuItem.IsChecked);
        }

        private void MainWind_Loaded(object sender, RoutedEventArgs e)
        {
            //mc.ConnectGamepad();
        }

    }
}