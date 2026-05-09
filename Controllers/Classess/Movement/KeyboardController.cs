using ARCA_WPF_F.Controllers.Classess.Arduino;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace ARCA_WPF_F.Controllers.Classess
{
    public class KeyboardController
    {
        private DataStruct data;
        private DispatcherTimer timer;
        private bool previousF1State = false;

        public event Action<DataStruct> DataReceived;

        public KeyboardController()
        {
            data = new DataStruct();

            // Setup timer for smooth keyboard polling (100 times per second)
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += OnTimerTick;
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();

            // Reset data to prevent the car from driving when stopped
            data.ResetData();
            DataReceived?.Invoke(data);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            UpdateControlInputs();
        }

        private void UpdateControlInputs()
        {
            // STEERING - A / D or Left / Right Arrows
            // We check for opposing keys to prevent glitching (defaults to center if both are pressed)
            bool isLeftPressed = Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left);
            bool isRightPressed = Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right);

            if (isLeftPressed && !isRightPressed)
            {
                data.steer = 0; // Max Left
            }
            else if (isRightPressed && !isLeftPressed)
            {
                data.steer = 255; // Max Right
            }
            else
            {
                data.steer = 127; // Center
            }

            // ACCELERATE - W or Up Arrow
            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up))
            {
                data.accelerate = 255;
            }
            else
            {
                data.accelerate = 0;
            }

            // BRAKE / REVERSE - S or Down Arrow
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down))
            {
                data.brake = 255;
            }
            else
            {
                data.brake = 0;
            }

            // AUXILIARY FUNCTIONS

            // F1 - Spacebar (Toggle Mode - changes state only once per press)
            bool currentF1State = Keyboard.IsKeyDown(Key.Space);
            if (currentF1State && !previousF1State)
            {
                data.F1 = !data.F1;
            }
            previousF1State = currentF1State;

            // F2 - Shift (Hold Mode - active only while pressed)
            data.F2 = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            // Send updated data
            DataReceived?.Invoke(data);
        }
    }
}