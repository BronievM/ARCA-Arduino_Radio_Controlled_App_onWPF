using ARCA_WPF_F.Controllers;
using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InputMode = ARCA_WPF_F.Controllers.Classess.InputMode;

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

            // Camera IP setup
            if (!string.IsNullOrWhiteSpace(main.GetCameraIP()))
            {
                IPTextBox.Text = main.GetCameraIP();
            }

            // Arduino setup
            ArduinoListComboBox.ItemsSource = main.ListArduino();
            ArduinoListComboBox.SelectedIndex = 0;

            if (main.CheckArduinoStatus() && main.GetUsedSP() != null)
            {
                string[] ports = SerialPort.GetPortNames();
                int selectedIndex = Array.IndexOf(ports, main.GetUsedSP().PortName);

                if (selectedIndex != -1)
                {
                    ArduinoListComboBox.SelectedIndex = selectedIndex;
                }
                ConnectButton.Content = "Disconnect";
            }
            else
            {
                ConnectButton.Content = "Connect";
            }

            // Gamepad setup
            ControllersComboBox.ItemsSource = main.GetGamepadList();
            ControllersComboBox.SelectedIndex = 0;

            UpdateInputUI();
        }

        public MainController GetMainController()
        {
            return this.main;
        }

        // ==========================================
        // CAMERA IP SETTINGS
        // ==========================================

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

        // ==========================================
        // WINDOW EVENTS
        // ==========================================

        private void Button_Click(object sender, RoutedEventArgs e) // Close Window Button
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Save();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // Save Button
        {
            Save();
        }

        private void UpdateDynUI()
        {
            ArduinoListComboBox.ItemsSource = main.ListArduino();
            ControllersComboBox.ItemsSource = main.GetGamepadList();

            if (main.CheckArduinoStatus()) { ConnectButton.Content = "Disconnect"; }
            else { ConnectButton.Content = "Connect"; }

            UpdateInputUI();
        }

        // ==========================================
        // ARDUINO CONNECTION
        // ==========================================

        private void ArduinoListComboBox_DropDownOpened(object sender, EventArgs e)
        {
            UpdateDynUI();
        }

        private void ArduinoListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Empty intentionally to prevent Bluetooth lag
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Arduino Connect/Disconnect
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
                    else if (string.IsNullOrEmpty(ArduinoListComboBox.Items[0]?.ToString()) || ArduinoListComboBox.Items[0].ToString().Contains("not found"))
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
                main.DisconnectArduino();
                ArduinoListComboBox.ItemsSource = main.ListArduino();
                MessageBox.Show("Disconnected!");
                ArduinoListComboBox.SelectedIndex = 0;
                ConnectButton.Content = "Connect";
            }
        }

        // ==========================================
        // MOVEMENT CONTROLS (GAMEPAD & KEYBOARD)
        // ==========================================

        private void ControllersComboBox_DropDownOpened(object sender, EventArgs e)
        {
            // Правильна подія для оновлення списку геймпадів при відкритті ComboBox
            ControllersComboBox.ItemsSource = main.GetGamepadList();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) // Gamepad Connect/Disconnect
        {
            if (main.GetCurrentInputMode() == ARCA_WPF_F.Controllers.Classess.InputMode.Gamepad)
            {
                main.DisconnectGamepad();
                MessageBox.Show("Gamepad disconnected.");
            }
            else
            {
                if (ControllersComboBox.SelectedItem != null && ControllersComboBox.SelectedIndex != -1)
                {
                    main.ConnectGamepad(ControllersComboBox.SelectedItem.ToString());
                }
                else
                {
                    MessageBox.Show("Please select a gamepad from the list.");
                }
            }
            UpdateInputUI();
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (main.GetCurrentInputMode() == InputMode.Keyboard)
            {
                main.DisconnectKeyboard();
                MessageBox.Show("Keyboard control disabled.");
            }
            else
            {
                main.ConnectKeyboard();
                MessageBox.Show("Keyboard control enabled! Use WASD or Arrows. Space - F1, Shift - F2");
            }
            UpdateInputUI();
        }

        private void UpdateInputUI()
        {
            /* GamepadConnectButton.Content = (main.GetCurrentInputMode() == InputMode.Gamepad) ? "Disconnect Gamepad" : "Connect Gamepad";
             * KeyboardConnectButton.Content = (main.GetCurrentInputMode() == InputMode.Keyboard) ? "Disable Keyboard" : "Enable Keyboard";
             */
        }

    }
}