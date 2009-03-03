﻿using System;
using NetGore.IO.Bits;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetGore.Network
{
    public interface ISocketSender
    {
        /// <summary>
        /// Asynchronously sends data to the socket.
        /// </summary>
        /// <param name="sourceStream">BitStream containing the data to send.</param>
        void Send(BitStream sourceStream);
    }
}