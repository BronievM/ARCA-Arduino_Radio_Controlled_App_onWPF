using System;
using System.Collections.Generic;
using ARCA_WPF_F.Controllers.Classess.Arduino;

namespace ARCA_WPF_F.Controllers.Classess
{
    public enum InputMode
    {
        None,
        Keyboard,
        Gamepad
    }

    public class MovementController
    {
        private KeyboardController keyboardController;
        private GamepadController gamepadController;
        private DataStruct dataStruct;

        public bool IsControllerConnected = false;
        public InputMode CurrentMode { get; private set; } = InputMode.None;

        public event Action<DataStruct> DataUpdated;

        public MovementController()
        {
            gamepadController = new GamepadController();
            keyboardController = new KeyboardController();

            // Subscribe to both controllers using a single event handler
            gamepadController.DataReceived += OnInputDataReceived;
            keyboardController.DataReceived += OnInputDataReceived;
        }

        // Centralized data handler
        private void OnInputDataReceived(DataStruct data)
        {
            dataStruct = data;
            DataUpdated?.Invoke(data);
        }

        // ==========================================
        // GAMEPAD CONTROL
        // ==========================================

        public void ConnectController(string id)
        {
            // If keyboard is active, stop it before switching to gamepad
            if (CurrentMode == InputMode.Keyboard)
            {
                StopKeyboard();
            }

            IsControllerConnected = gamepadController.ConnectToController(id);
            if (IsControllerConnected)
            {
                CurrentMode = InputMode.Gamepad;
            }
        }

        public void DisconnectController()
        {
            gamepadController.Disconnect();
            IsControllerConnected = false;

            if (CurrentMode == InputMode.Gamepad)
            {
                CurrentMode = InputMode.None;
                ResetData(); // Send zero-data to stop the car
            }
        }

        public List<string> ListAvailableControllers()
        {
            return gamepadController.GetAvailableControllerNames();
        }

        // ==========================================
        // KEYBOARD CONTROL
        // ==========================================

        public void StartKeyboard()
        {
            // Disconnect gamepad if trying to use keyboard
            if (CurrentMode == InputMode.Gamepad && IsControllerConnected)
            {
                DisconnectController();
            }

            keyboardController.Start();
            CurrentMode = InputMode.Keyboard;
        }

        public void StopKeyboard()
        {
            keyboardController.Stop();

            if (CurrentMode == InputMode.Keyboard)
            {
                CurrentMode = InputMode.None;
            }
        }


        // ==========================================
        // UTILITY
        // ==========================================

        public void ResetData()
        {
            dataStruct.ResetData();
            DataUpdated?.Invoke(dataStruct);
        }
    }
}