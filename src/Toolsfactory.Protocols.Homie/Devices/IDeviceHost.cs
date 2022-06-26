using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Toolsfactory.Protocols.Homie.Devices
{
    public interface IDeviceHost
    {
        bool IsConnected { get; }
        bool IsStarted { get; }

        ILogger<IDeviceHost> Logger { get; }

        Task StartAsync();
        Task StopAsync();

        Task<bool> TryConnectAsync();
        Task PublishAsync(string topic, string payload, bool retained = true);
        Task SubscribeAsync(string topic, Action<string, string> handler);
        Task UnsubscribeAsync(string topic);
    }
}