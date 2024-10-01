using ARCA_WPF_F.Controllers;
using ARCA_WPF_F.Controllers.Classess.Arduino;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ARCA_WPF_F.Resources
{
    public partial class SettingsWindow : Window
    {
        private MainController main;
        private bool IPChanged;
        private string previousText;

        public SettingsWindow(MainController main)
        {
            InitializeComponent();
            this.main = main;
          
            if (!string.IsNullOrWhiteSpace(main.GetCameraIP()))
            {
                IPTextBox.Text = main.GetCameraIP();
            }

            ArduinoListComboBox.ItemsSource = main.ListArduino();
            ArduinoListComboBox.SelectedIndex = 0;

            if(main.CheckArduinoStatus())
            {
                string[] ports = SerialPort.GetPortNames();
                int selectedIndex = -1;
                for (int i = 0; i < ports.Length; i++)
                {
                    if (ports[i] == main.GetUsedSP().PortName)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                ArduinoListComboBox.SelectedIndex = selectedIndex;
                ConnectButton.Content = "Disconnect"; 
              
            }
            else { ConnectButton.Content = "Connect"; }

            ControllersComboBox.ItemsSource = main.GetGamepadList();
            ControllersComboBox.SelectedIndex = 0;
        }

        public MainController GetMainController()
        {
            return this.main;
        }

        private void IPTextBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                Save();
            }

            else if (e.Key == Key.Escape)
            {
                if (!IPChanged) return;
                if (MessageBox.Show("Are you sure that box will clear?", "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    IPTextBox.Text = null;
                }
            }
            else
            {
                IPChanged = true;
            }
        }

        private void Save()
        {
            if (IPChanged)
            {
                if (String.IsNullOrEmpty(IPTextBox.Text)) MessageBox.Show("Ip successfully deleted");
                else MessageBox.Show("Success: '" + IPTextBox.Text + "' is set");
            }

            main.SetCameraIP(IPTextBox.Text);
            IPChanged = false;
        }

        private void IPTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            previousText = IPTextBox.Text;
        }

        private void IPTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (IPTextBox.Text != previousText)
            {
                IPChanged = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Save();
        }

        private void UpdateDynUI()
        {
            ArduinoListComboBox.ItemsSource = main.ListArduino();
            ControllersComboBox.ItemsSource = main.GetGamepadList();
            if (main.CheckArduinoStatus()) { ConnectButton.Content = "Disconnect"; }
            else { ConnectButton.Content = "Connect"; }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            bool isArduinoConnected = main.CheckArduinoStatus();

            if (!isArduinoConnected)
            {
                try
                {
                    if (ArduinoListComboBox.SelectedItem == null || ArduinoListComboBox.SelectedIndex == -1)
                    {
                        MessageBox.Show("No COM port selected!");
                        return;
                    }
                    else if (string.IsNullOrEmpty(ArduinoListComboBox.Items[0]?.ToString()))
                    {
                        ArduinoListComboBox.SelectedIndex = 0;
                        MessageBox.Show("Arduino not found, connect it to pc");
                        return;
                    }
                    else
                    {
                        int selectedArduinoIndex = ArduinoListComboBox.SelectedIndex;
                        string portName = main.GetPortIDArduinoByLocalID(selectedArduinoIndex);

                        string[] ports = SerialPort.GetPortNames();
                        if (!ports.Contains(portName))
                        {
                            MessageBox.Show($"The port '{portName}' does not exist.");
                            return;
                        }

                        bool isConnected = main.ConnectArduino(portName);
                        if (isConnected)
                        {
                            ConnectButton.Content = "Disconnect";
                            MessageBox.Show("Connected!");
                            return;
                        }
                        else
                        {
                            UpdateDynUI();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                if (isArduinoConnected)
                {
                    main.DisconnectArduino();
                    ArduinoListComboBox.ItemsSource = main.ListArduino();
                    MessageBox.Show("Disconnected!");
                    ArduinoListComboBox.SelectedIndex = 0;
                    ConnectButton.Content = "Connect";
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void ArduinoListComboBox_DropDownOpened(object sender, EventArgs e)
        {
            UpdateDynUI();
        }

        private void ArduinoListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UpdateDynUI(); - Сповільнювало роботу при ввімкненому Bluetooth
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (ControllersComboBox.SelectedItem != null && ControllersComboBox.SelectedIndex != -1)
            {
                main.ConnectGamepad(ControllersComboBox.SelectedItem.ToString());
            }
        }

        private void ControllersComboBox_Drop(object sender, EventArgs e)
        {
            ControllersComboBox.ItemsSource = main.GetGamepadList();
        }

        private void ControllersComboBox_Drop_1(object sender, DragEventArgs e)
        {
            ControllersComboBox.ItemsSource = main.GetGamepadList();
        }
    }
}
