using System;
using Microsoft.Extensions.Logging;

namespace Toolsfactory.Protocols.Homie.Devices
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> startReceive = LoggerMessage.Define(LogLevel.Information, new EventId(100001), "");
        private static readonly Action<ILogger, Exception> connectingFailed = LoggerMessage.Define(LogLevel.Error, new EventId(100002), "Connecting to MQTT broker failed");

        public static void XLogStartReceive(this ILogger logger)
        {
            startReceive(logger, null);
        }

        public static void XLogConnectingFailed(this ILogger logger, Exception ex)
        {
            connectingFailed(logger, ex);
        }

    }
}

