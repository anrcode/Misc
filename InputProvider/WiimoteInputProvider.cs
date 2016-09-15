using System;

using WiimoteLib;


namespace Misc.InputProvider
{
    /// <summary>
    /// This class captures input coming from a Wiimote controller.
    /// </summary>
    public class WiimoteInputProvider : IInputProvider
    {
        // properties
        private Wiimote _wiimote = new Wiimote();

        // events
        public event EventHandler<InputProviderEventArgs> OnInputDataUpdate;


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="droneCommander">The drone commander.</param>
        public WiimoteInputProvider()
        {
            _wiimote.WiimoteExtensionChanged += new EventHandler<WiimoteExtensionChangedEventArgs>(wm_WiimoteExtensionChanged);
            _wiimote.WiimoteChanged += wm_WiimoteChanged;
        }

        //// <summary>
        /// Initializes the specified input provider.
        /// </summary>
        /// <returns>
        /// Returns true if the provider was initialized correclty; otherwise, false.
        /// </returns>
        public bool Initialize()
        {
            try
            {
                _wiimote.Connect();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SetLEDs(bool led1, bool led2, bool led3, bool led4)
        {
            _wiimote.SetLEDs(false, false, true, false);
        }

        protected void wm_WiimoteExtensionChanged(object sender, WiimoteExtensionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Handles the WiimoteChanged event of the wm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WiimoteLib.WiimoteChangedEventArgs"/> instance containing the event data.</param>
        protected void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            float height = 0;
            float yaw = 0;

            float roll = e.WiimoteState.AccelState.Values.X;
            float pitch = -e.WiimoteState.AccelState.Values.Y;

            if (this.OnInputDataUpdate != null)
            {
                InputProviderEventArgs args = new InputProviderEventArgs();
                args.AccelX = e.WiimoteState.AccelState.Values.X;
                args.AccelY = -e.WiimoteState.AccelState.Values.Y;
                args.AccelZ = e.WiimoteState.AccelState.Values.Z;
                this.OnInputDataUpdate(this, args);
            }

            if (e.WiimoteState.ButtonState.Up)
            {
                height = 1.0f;
            }
            else if (e.WiimoteState.ButtonState.Down)
            {
                height = -1.0f;
            }
            else
            {
                height = 0;
            }

            if (e.WiimoteState.ButtonState.Left)
            {
                yaw = -1.0f;
            }
            else if (e.WiimoteState.ButtonState.Right)
            {
                yaw = 1.0f;
            }
            else
            {
                yaw = 0;
            }

            //DroneCommander.SetFlightParameters(roll, pitch, height, yaw);

            if (e.WiimoteState.ButtonState.A)
            {
                //DroneCommander.StartEngines();
            }

            if (e.WiimoteState.ButtonState.B)
            {
                //DroneCommander.StopEngines();
            }

            if (e.WiimoteState.ButtonState.Minus)
            {
                //DroneCommander.PlayLedAnimation(animationId, 10, 3);
            }

            if (e.WiimoteState.ButtonState.Plus)
            {
                //DroneCommander.DisplayNextVideoChannel();
            }

            if (e.WiimoteState.ButtonState.One)
            {
                //DroneCommander.StartRecordVideo();
            }

            if (e.WiimoteState.ButtonState.Two)
            {
                //DroneCommander.StopRecordVideo();
            }

            if (e.WiimoteState.ButtonState.Home)
            {
                //DroneCommander.TakePicture();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_wiimote != null)
            {
                _wiimote.Disconnect();
                _wiimote.Dispose();
                _wiimote = null;
            }
        }     
    }
}
