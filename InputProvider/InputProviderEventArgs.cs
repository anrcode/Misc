using System;


namespace Misc.InputProvider
{
    public class InputProviderEventArgs : EventArgs
    {
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float Z { get; internal set; }

        public float AccelX { get; internal set; }
        public float AccelY { get; internal set; }
        public float AccelZ { get; internal set; }
    }
}
