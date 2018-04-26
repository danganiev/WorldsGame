using System;

namespace WorldsGame.Network
{
    /// <summary>
    /// A truly exceptonal situation where server just disappeared, and didn't send the disconnect message.
    /// </summary>
    internal class ServerConnectionLostException : Exception
    {
        internal ServerConnectionLostException()
        {
        }

        internal ServerConnectionLostException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}