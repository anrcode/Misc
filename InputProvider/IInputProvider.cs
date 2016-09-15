using System;


namespace Misc.InputProvider
{
    /// <summary>
    /// Provides a connection between an input device (e.g. Joystick) and an object that implements the <see cref="IDroneCommander">IDroneCommander</see> interface.
    /// </summary>
    public interface IInputProvider : IDisposable
    {
        event EventHandler<InputProviderEventArgs> OnInputDataUpdate;

        /// <summary>
        /// Initializes the specified input provider.
        /// </summary>
        /// <returns>
        /// Returns true if the provider was initialized correclty; otherwise, false.
        /// </returns>
        bool Initialize();
    }
}
