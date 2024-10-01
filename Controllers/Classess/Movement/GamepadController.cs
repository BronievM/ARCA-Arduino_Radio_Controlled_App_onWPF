using ARCA_WPF_F.Controllers.Classess.Arduino;
using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace ARCA_WPF_F.Controllers.Classess
{
    public class GamepadController
    {
        private Joystick connectedController;
        private DirectInput directInput;
        private DataStruct data;
        private bool IsXbox = false;
        private bool previousButton0State = false;
        private bool IsControllerConnected = false;

        public event Action<JoystickUpdate[]> ControlInputsUpdated;
        public event Action<DataStruct> DataReceived;

        public GamepadController()
        {
            directInput = new DirectInput();
            data = new DataStruct();
        }

        public bool ConnectToController(string controllerName)
        {
            var availableControllerNames = GetAvailableControllerNames();

            if (!availableControllerNames.Contains(controllerName) || controllerName == "No controllers found")
            {
                MessageBox.Show("Specified controller not found.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                var result = MessageBox.Show($"Do you want to connect to controller '{controllerName}'?", "Confirm Connection", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var deviceInstance = directInput.GetDevices()
                        .FirstOrDefault(device => device.ProductName == controllerName);

                    if (deviceInstance == null)
                    {
                        MessageBox.Show("Could not find device instance for the specified controller.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsControllerConnected = false;
                        return false;
                    }

                    if (ConnectToJoystick(deviceInstance.InstanceGuid))
                    {
                        MessageBox.Show($"Connected to controller: {controllerName}", "Connection Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        SetupTimer();
                        IsControllerConnected = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to the controller: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        private bool ConnectToJoystick(Guid controllerGuid)
        {
            try
            {
                connectedController = new Joystick(directInput, controllerGuid);

                if (connectedController.Information.InstanceName.ToUpper().Contains("X") || connectedController.Information.InstanceName.ToUpper().Contains("XBOX"))
                {
                    IsXbox = true;
                }
                else IsXbox = false;
                connectedController.Properties.BufferSize = 128;
                connectedController.Acquire();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private void SetupTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (connectedController != null)
                {
                    JoystickUpdate[] datas = connectedController.GetBufferedData();
                    UpdateControlInputs(datas);
                    OnControlInputsUpdated(datas);
                }
            }
            catch (SharpDXException ex)
            {
                if (ex.HResult == unchecked((int)0x8007001E)) 
                {
                    Disconnect();
                    MessageBox.Show("Controller disconnected");
                }
                else
                {
                    MessageBox.Show($"Error reading data from the controller: {ex.Message}");
                }
            }
        }


        public void Disconnect()
        {
            if (connectedController != null)
            {
                connectedController.Unacquire();
                connectedController = null;
            }
        }


        public List<string> GetAvailableControllerNames()
        {
            var controllerNames = new List<string>();

            var devices = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices)
                                    .Concat(directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices));

            foreach (var deviceInstance in devices)
            {
                controllerNames.Add(deviceInstance.ProductName);
            }

          if (controllerNames.Count == 0) controllerNames.Add("No controllers found");

            return controllerNames;
        }

        private void OnControlInputsUpdated(JoystickUpdate[] datas)
        {
            ControlInputsUpdated?.Invoke(datas);
        }

        private void UpdateControlInputs(JoystickUpdate[] datas)
        {
            foreach (var state in datas)
            {
                var effects = connectedController.GetEffects();
                switch (state.Offset)
                {
                    case JoystickOffset.X:
                        data.steer = (byte)(state.Value * 255 / 65535);
                        break;

                    case JoystickOffset.Z:
                        if (IsXbox)
                        {
                            int zValue = state.Value;
                            if (zValue >= 32767) data.accelerate = 0;
                            else data.accelerate = (byte)((32767 - zValue) * 255 / 32767);
                        }
                        break;

                    // in case of PS4
                    case JoystickOffset.RotationY:
                        if (!IsXbox)
                        {
                            data.accelerate = (byte)(state.Value / 256);
                        }
                        break;

                    case JoystickOffset.RotationX:
                        if (!IsXbox)
                        {
                            data.brake = (byte)(state.Value / 256);
                        }
                        break;

                        //

                    case JoystickOffset.Buttons0:
                        bool currentButton0State = state.Value != 0;
                        if (!previousButton0State && currentButton0State)
                        {
                            data.F1 = !data.F1;
                            OnDataReceived(data);
                        }
                        break;

                    case JoystickOffset.Buttons1:
                        data.F2 = state.Value != 0;
                        break;
                }
            }
           
            OnDataReceived(data);
        }

        private void OnDataReceived(DataStruct data) { 
            DataReceived?.Invoke(data);
        }
    }

}
