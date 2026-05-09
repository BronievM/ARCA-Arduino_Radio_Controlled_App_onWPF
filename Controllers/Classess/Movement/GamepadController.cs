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

        // Constants for deadzones (stick drift cutoff)
        private const int AxisCenter = 32767;
        private const int Deadzone = 5000;
        private const int AxisMin = AxisCenter - Deadzone; // 27767
        private const int AxisMax = AxisCenter + Deadzone; // 37767

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
                    if (datas.Length > 0)
                    {
                        UpdateControlInputs(datas);
                        OnControlInputsUpdated(datas);
                    }
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
                int val = state.Value;

                switch (state.Offset)
                {
                    // LEFT STICK (Steering)
                    case JoystickOffset.X:
                        if (val > AxisMin && val < AxisMax)
                        {
                            data.steer = 127; // Deadzone (dead center)
                        }
                        else
                        {
                            data.steer = (byte)(val * 255 / 65535);
                        }
                        break;

                    // TRIGGERS OR RIGHT STICK (Depends on the controller)
                    case JoystickOffset.Z:
                        if (IsXbox)
                        {
                            // Logic for Xbox (Triggers or Z axis)
                            if (val >= 32767) data.accelerate = 0;
                            else data.accelerate = (byte)((32767 - val) * 255 / 32767);
                        }
                        break;

                    // RIGHT STICK (Y axis) or PS4 TRIGGERS
                    case JoystickOffset.RotationY:
                        if (IsXbox)
                        {
                            // Xbox right stick: Up - Accelerate, Down - Brake
                            if (val < AxisMin) // Stick pushed up
                            {
                                data.accelerate = (byte)((AxisMin - val) * 255 / AxisMin);
                                data.brake = 0;
                            }
                            else if (val > AxisMax) // Stick pushed down
                            {
                                data.brake = (byte)((val - AxisMax) * 255 / (65535 - AxisMax));
                                data.accelerate = 0;
                            }
                            else // Deadzone
                            {
                                data.accelerate = 0;
                                data.brake = 0;
                            }
                        }
                        else
                        {
                            // PS4 Logic (L2 Trigger)
                            data.accelerate = (byte)(val / 256);
                        }
                        break;

                    case JoystickOffset.RotationX:
                        if (!IsXbox)
                        {
                            // PS4 Logic (R2 Trigger)
                            data.brake = (byte)(val / 256);
                        }
                        break;

                    // BUTTONS
                    case JoystickOffset.Buttons0:
                        bool currentButton0State = val != 0;
                        if (!previousButton0State && currentButton0State)
                        {
                            data.F1 = !data.F1;
                            OnDataReceived(data); // Force send on state change
                        }
                        previousButton0State = currentButton0State; // Update previous state
                        break;

                    case JoystickOffset.Buttons1:
                        data.F2 = val != 0;
                        break;
                }
            }

            OnDataReceived(data);
        }

        private void OnDataReceived(DataStruct data)
        {
            DataReceived?.Invoke(data);
        }
    }
}