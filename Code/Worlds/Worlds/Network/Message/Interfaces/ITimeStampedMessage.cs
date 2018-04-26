using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Network.Message
{
    interface ITimeStampedMessage
    {
        double Timestamp { get; set; }
    }
}
