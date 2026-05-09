using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Windows;
using ARCA_WPF_F.Controllers.Classess.Arduino;

namespace ARCA_WPF_F.Controllers.Classess
{
    public class ArduinoController
    {
        private DataStruct dataStruct;
        private List<Com> comlist;
        private SerialPort UsedPort;
        private bool IsArduinoConnected;
        private bool IsErrorDisconnectedBeen;
        private bool IsNRFConnected;
        private bool IsDataReceived;
        private System.Timers.Timer dataTimer;
        private DateTime lastDataSent;

        public delegate void DataSentHandler(DateTime time, bool IsNRFConnected, bool IsDataReceived);
        public event DataSentHandler DataSent;

        public delegate void DataUpdatedHandler(DataStruct data);
        public event DataUpdatedHandler DataUpdated;


        public ArduinoController()
        {
            dataStruct = new DataStruct();
            comlist = new List<Com>();

            dataTimer = new System.Timers.Timer(100);
            dataTimer.Elapsed += OnTimedEvent;
            dataTimer.AutoReset = true;
            dataTimer.Enabled = false;
        }


        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            SendDataToArduino();
        }

        public void StartSendingData()
        {
            dataTimer.Enabled = true;
        }

        public void StopSendingData()
        {
            dataTimer.Enabled = false;
        }

        public bool ConnectToArduino(string portID)
        {       
            try
            {
                UsedPort = new SerialPort(portID, 9600);
                UsedPort.Open(); 
                UsedPort.DtrEnable = true;
                UsedPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                IsArduinoConnected = true;
                StartSendingData();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open serial port: {ex.Message}");
                return false ;
            }
        }


        public bool CheckStatus()
        {
            return IsArduinoConnected;
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedData = UsedPort.ReadLine();
            switch (receivedData)
            {
                case "DS\r":
                    IsDataReceived = true;
                    break;
                case "DS:E\r":
                    IsDataReceived = false;
                    break;
                case "nRFC\r":
                    IsNRFConnected = true; 
                    break;
                case "nRFNC\r":
                    IsDataReceived = false;
                    MessageBox.Show("There is a problem with radiomodule", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    MessageBox.Show("Data received:\n" + receivedData);
                    break;
            }
        }

        public void OnDataUpdated(DataStruct data)
        {
            DataUpdated?.Invoke(data);
        }

        public void DisconnectArduino()
        {
            if (IsArduinoConnected && UsedPort != null)
            {
                try {
                    StopSendingData();
                    UsedPort.Close();
                    IsArduinoConnected = false;
                }
                catch { }
            }
        }

        public List<Com> ListArduinoComs()
        {
            comlist = new List<Com>();
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length == 0) return comlist;

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'"))
                {
                    var managementObjects = searcher.Get();

                    foreach (string port in ports)
                    {
                        bool isBluetooth = false;
                        string description = port + " (Info not found)";

                        foreach (ManagementObject queryObj in managementObjects)
                        {
                            string caption = queryObj["Caption"]?.ToString() ?? "";
                            string desc = queryObj["Description"]?.ToString() ?? "Unknown Device";

                            if (caption.Contains($"({port})"))
                            {
                                if (caption.Contains("Bluetooth") || desc.Contains("Bluetooth") || caption.Contains("BTHENUM"))
                                {
                                    isBluetooth = true;
                                }
                                else
                                {
                                    description = $"{port} ({desc})";
                                }
                                break;
                            }
                        }

                        if (!isBluetooth)
                        {
                            comlist.Add(new Com(port, description));
                        }
                    }
                }
            }
            catch
            {
                foreach (string port in ports)
                {
                    comlist.Add(new Com(port, port + " (Unknown)"));
                }
            }

            return comlist;
        }

        public Com GetComByLocalID(int ID)
        {
            List<Com> list = ListArduinoComs();
            if (ID >= 0 && ID < list.Count)
            {
                return comlist[ID];
            }
            return null;
        }

        public string GetComPortDescription(string portName)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%({portName})%'"))
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        string desc = queryObj["Description"]?.ToString() ?? "Unknown Device";
                        return $"{portName} ({desc})";
                    }
                }
            }
            catch { }

            return portName + " (Info not found)";
        }

        public SerialPort GetUsedSerialPort()
        {
            return UsedPort;
        }

        public DateTime GetLastDataSentTime()
        {
            return lastDataSent;
        }

        public bool IsUsedPortBusy()
        {
            if (IsArduinoConnected)
            {
                return false;
            }

            if (UsedPort == null)
            {
                return false;
            }

            try
            {
                UsedPort.Open();
                UsedPort.Close();
                return false;
            }
            catch
            {
                return true;
            }
        }

        public void ChangeData(DataStruct data)
        {
            dataStruct.SetData(data.steer, data.accelerate, data.brake, data.F1, data.F2);
        }

        public void ResetData()
        {
            dataStruct.ResetData();
        }

        public void SendDataToArduino()
        {
            try
            {
                if (UsedPort != null)
                {
                    if (!UsedPort.IsOpen)
                    {
                        if (!IsErrorDisconnectedBeen)
                        {
                            DisconnectArduino();
                            IsErrorDisconnectedBeen = true;
                            lastDataSent = DateTime.Now;
                            DataSent?.Invoke(lastDataSent, IsNRFConnected, IsDataReceived);
                            MessageBox.Show("There is some problem with arduino connection\nTry to reconnect it (in settings window)", "Arduino disconnected!");
                            comlist = new List<Com>();
                        }
                        return;
                    }

                    const int DataSize = 5;
                    byte[] data = new byte[DataSize];
                    data[0] = (byte)dataStruct.steer;
                    data[1] = (byte)dataStruct.accelerate;
                    data[2] = (byte)dataStruct.brake;
                    data[3] = (byte)(dataStruct.F1 ? 1 : 0);
                    data[4] = (byte)(dataStruct.F2 ? 1 : 0);

                    UsedPort.Write(data, 0, data.Length);
                    lastDataSent = DateTime.Now;
                    DataSent?.Invoke(lastDataSent, IsNRFConnected, IsDataReceived);
                    IsErrorDisconnectedBeen = false;

                }
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is UnauthorizedAccessException)
                {
                    IsErrorDisconnectedBeen = true;
                }
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
