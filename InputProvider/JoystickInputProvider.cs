using System;
using System.Timers;

using SlimDX.DirectInput;
using SlimDX;


namespace Misc.InputProvider
{
    /// <summary>
    /// This class captures input coming from an analog joystick.
    /// </summary>
    public class JoystickInputProvider : IInputProvider
    {
        private Joystick joystick;
        private JoystickState currentState = new JoystickState();

        private int previousX;
        private int previousY;
        private int previousRotationZ;
        private int previousSlider;

        private Single progressivePitch;
        private Single progressiveRoll;
        private Single progressiveGaz;
        private Single progressiveYaw;

        private Single picthThrottleValue;
        private Single rollThrottleValue;
        private Single heightThrottleValue;
        private Single yawThrottleValue;

        /// <summary>
        /// Gets or sets the joy stick range (the maximum value a joystick produces for a specific movement).
        /// </summary>
        /// <value>The joy stick range.</value>
        public int JoyStickRange { get; set; }

        private Timer InputTimer { get; set; }

        public event EventHandler<InputProviderEventArgs> OnInputDataUpdate;


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public JoystickInputProvider()
        {
        }

        /// <summary>
        /// Initializes the specified input provider.
        /// </summary>
        /// <returns>
        /// Returns true if the provider was initialized correclty; otherwise, false.
        /// </returns>
        public bool Initialize()
        {
            try
            {
                // make sure that DirectInput has been initialized
                DirectInput dinput = new DirectInput();

                // search for devices
                foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
                {
                    joystick = new Joystick(dinput, device.InstanceGuid);
                    break;
                }

                if (joystick != null)
                {
                    foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
                    {
                        if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                            joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);
                    }

                    // acquire the device
                    joystick.Acquire();

                    // set the timer to go off 12 times a second to read input
                    // NOTE: Normally applications would read this much faster.
                    // This rate is for demonstration purposes only.
                    InputTimer = new Timer(1000 / 12);
                    InputTimer.Elapsed += new ElapsedEventHandler(InputTimer_Elapsed);
                    InputTimer.Start();
                }
                else
                {
                    throw new Exception("There are no joysticks attached to the system.");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the InputTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void InputTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (joystick.Acquire().IsFailure)
                return;

            if (joystick.Poll().IsFailure)
                return;
            
            currentState = joystick.GetCurrentState();
            if (Result.Last.IsFailure)
                return;


            if (this.OnInputDataUpdate != null)
            {
                InputProviderEventArgs args = new InputProviderEventArgs();
                args.X = (float)currentState.X * 1.0f / (float)this.JoyStickRange;
                args.Y = (float)currentState.Y * 1.0f / (float)this.JoyStickRange;
                args.Z = (float)currentState.Z * 1.0f / (float)this.JoyStickRange;
                args.AccelX = (float)currentState.AccelerationX * 1.0f / (float)this.JoyStickRange;
                args.AccelY = (float)currentState.AccelerationY * 1.0f / (float)this.JoyStickRange;
                args.AccelZ = (float)currentState.AccelerationZ * 1.0f / (float)this.JoyStickRange;

                this.OnInputDataUpdate(this, args);
            }

            //if (currentState.Y != previousY)
            //{
            //    progressivePitch = (float)currentState.Y * picthThrottleValue / this.JoyStickRange;
            //}

            //if (currentState.X != previousX)
            //{
            //    progressiveRoll = (float)currentState.X * rollThrottleValue / this.JoyStickRange; 
            //}

            //if (currentState.RotationZ != previousRotationZ)
            //{
            //    progressiveYaw = (float)currentState.RotationZ * yawThrottleValue / this.JoyStickRange;
            //}

            int[] slider = currentState.GetSliders();

            if (slider[0] != previousSlider)
            {
                progressiveGaz = (float)slider[0] * heightThrottleValue / this.JoyStickRange;
            }

            bool[] buttons = currentState.GetButtons();

            if (buttons[0])
            {
                //DroneCommander.StopEngines();
            }

            if (buttons[1])
            {
                //DroneCommander.StartEngines();
            }

            if (buttons[2])
            {
                //DroneCommander.SetFlatTrim();
            }

            if (buttons[3])
            {
                //DroneCommander.SetFlatTrim();
            }

            if (buttons[6])
            {
                //DroneCommander.StartReset();
            }

            if (buttons[7])
            {
                //DroneCommander.StopReset();
            }

            //DroneCommander.SetFlightParameters(progressiveRoll, progressivePitch, - progressiveGaz, progressiveYaw);

            previousX = currentState.X;
            previousY = currentState.Y;
            previousRotationZ = currentState.RotationZ;
            previousSlider = slider[0];
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(InputTimer != null)
            {
                InputTimer.Stop();
            }

            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }

            joystick = null;
        }
    }
}
