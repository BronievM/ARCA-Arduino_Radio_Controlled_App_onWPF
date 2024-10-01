using System;
using System.Collections.Generic;
using ARCA_WPF_F.Controllers.Classess.Arduino;

namespace ARCA_WPF_F.Controllers.Classess
{
    internal class MovementController
    {
        private KeyboardController keyboardController;
        private GamepadController gamepadController;
        private DataStruct dataStruct;
        public bool IsControllerConnected = false;
        public event Action<DataStruct> DataUpdated;
        public MovementController() {
            gamepadController = new GamepadController();
            keyboardController = new KeyboardController();
        }

        public void ConnectController(string id)
        {
            IsControllerConnected = gamepadController.ConnectToController(id);
            gamepadController.DataReceived += GamepadController_DataUpdated;
        }

        public void GamepadController_DataUpdated(DataStruct data)
        {
           dataStruct = data;
            DataUpdated?.Invoke(data);
        }

        public void DisconnectController()
        {
            gamepadController.Disconnect();
            IsControllerConnected = false;
        }

        public List<string> ListAvailableControllers()
        {
            return gamepadController.GetAvailableControllerNames();
        }

      
    }

}

