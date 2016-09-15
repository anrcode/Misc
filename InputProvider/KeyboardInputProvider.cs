using System;

using SlimDX.Multimedia;
using SlimDX.RawInput;


namespace Misc.InputProvider
{
    public class KeyboardInputProvider : IInputProvider
    {
        public event EventHandler<InputProviderEventArgs> OnInputDataUpdate;


        public KeyboardInputProvider()
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
            Device.RegisterDevice(UsagePage.Generic, UsageId.Keyboard, DeviceFlags.None);
            Device.KeyboardInput += new EventHandler<KeyboardInputEventArgs>(KeyboardInput);

            return true;
        }

        protected void KeyboardInput(object sender, KeyboardInputEventArgs e)
        {
            Console.WriteLine("Keypress: " + e.Key);
        }

        public void Dispose()
        {

        }
    }
}
