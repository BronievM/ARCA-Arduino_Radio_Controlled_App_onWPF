using System;
using System.Collections.Generic;
using System.IO.Ports;
using ARCA_WPF_F.Controllers.Classess;
using ARCA_WPF_F.Controllers.Classess.Arduino;

namespace ARCA_WPF_F.Controllers
{
    public class MainController
    {
        private CameraController camera;
        private MovementController movement;
        private SettingsController settings;
        private ArduinoController arduino;
        public delegate void DataSentHandler(DateTime time, bool IsNRFConnected, bool IsDataReceived);
        public event DataSentHandler DataSent;
        public delegate void DataUpdatedHandler(DataStruct data);
        public event DataUpdatedHandler DataUpdated;

        public MainController()
        {
            camera = new CameraController();
            settings = SettingsController.LoadFromFile();
            arduino = new ArduinoController();
            movement = new MovementController();

            movement.DataUpdated += MovementController_DataUpdated;
            arduino.DataUpdated += Arduino_DataUpdated;

            arduino.DataSent += Arduino_DataSent;
        }

        public void Saving()
        {
            settings.SaveToFile();
        }

        public void SetDebug(bool IsDebugOpen)
        {
            settings.Debug = IsDebugOpen;
        }

        public bool GetDebug()
        {
            return settings.Debug;
        }

        // >------------------
        //      Arduino
        // >------------------

        public SerialPort GetUsedSP()
        {
            return arduino.GetUsedSerialPort();
        }

        public Com SelectArduinoByLocalID(int ID)
        {
            List<Com> list = arduino.ListArduinoComs();
            return list[ID];
        }

        public string GetPortIDArduinoByLocalID(int ID)
        {
            List<Com> list = arduino.ListArduinoComs();
            return list[ID].GetId();
        }

        public bool ConnectArduino(string PortID)
        {
           return arduino.ConnectToArduino(PortID);
        }

        public List<String> ListArduino()
        {

            List<String> list = new List<String>();
            List<Com> comList = new List<Com>();

            comList = arduino.ListArduinoComs();

            if (comList.Count != 0)
            {
                foreach (Com i in comList)
                {
                    list.Add(arduino.GetComPortDescription(i.GetId()));
                }
            }
            else list.Add("Arduino not found, connect it to pc");

            return list;
        }

        public bool CheckArduinoStatus()
        {
            return arduino.CheckStatus();
        }

        private void Arduino_DataSent(DateTime time, bool IsNRFConnected, bool IsDataReceived)
        {
            DataSent?.Invoke(time, IsNRFConnected, IsDataReceived); 
        }

        private void Arduino_DataUpdated(DataStruct data)
        {
           DataUpdated?.Invoke(data);
        }

        public void DisconnectArduino()
        {
            arduino.DisconnectArduino();
        }

        // >-------------------
        //       Camera
        // >-------------------
        public CameraController Camera
        {
            get { return camera; }
        }

        public void SetCameraIP(string IP)
        {
            settings.SaveIP(IP);
        }

        public void ConnectCamera()
        {
//            if (String.IsNullOrWhiteSpace(settings.GetIP())) return;
            camera.SetCameraIP(settings.GetIP());   
            camera.ConnectCamera();
        }

        public void ConnectCamera(string IP)
        {
            settings.SaveIP(IP);
            camera.SetCameraIP(settings.GetIP());
            camera.ConnectCamera();
        }

        public void DisconnectCamera()
        {
            camera.DisconnectCamera();
        }

        public bool CheckCameraStatus()
        {
            return camera.CheckCameraStatus();
        }

        public string GetCameraIP()
        {
            return camera.GetCameraIP();
        }

        // >--------------------
        //
        //       Movement
        //
        // >--------------------

        public void ConnectGamepad(string id)
        {
            movement.ConnectController(id);
        }

        public bool IsControllerConnected()
        {
            return movement.IsControllerConnected;
        }

        public List<string> GetGamepadList()
        {
            return movement.ListAvailableControllers();
        }

        public void DisconnectGamepad()
        {
            movement.DisconnectController();
        }

        private void MovementController_DataUpdated(DataStruct data)
        {
            arduino.ChangeData(data);
            arduino.OnDataUpdated(data);
        }
    }
}
