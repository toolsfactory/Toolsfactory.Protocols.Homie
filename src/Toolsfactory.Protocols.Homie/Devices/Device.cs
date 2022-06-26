using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using System.Net.NetworkInformation;
using System.Text;

namespace Toolsfactory.Protocols.Homie.Devices
{
    public delegate void BroadcastReceivedDelegate(string topic, string newvalue);
    public class Device : HomieElement
    {
        public DeviceState State { get; private set; } = DeviceState.Init;
        public IReadOnlyDictionary<string, Node> Nodes { get => _nodes; }
        public IDeviceHost? Host 
        { 
            get => _host;
            set
            {
                if (_host == null && value != null)
                {
                    _host = value;
                }
            }
        }

        public BroadcastReceivedDelegate? BroadcastReceived { get; set; }


        private readonly Dictionary<string, Node> _nodes = new();
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private CancellationTokenSource? _cancelToken;
        private Task? _stateMachineTask;
        private IDeviceHost? _host;
        private DateTime _startTime;

        public Device(string id, string name)
            : base(id, name)
        {
            Topic = $"{Constants.TopicRoot}/{Id}";
            _startTime = DateTime.Now;
            _timer.Elapsed += _timer_Elapsed;
        }

        public Node AddNode(string id, string @type="default", string friendlyName = "default")
        {
            if ((_host != null) && _host.IsStarted)
                throw new InvalidOperationException("Adding nodes only possible when host not started!");
            if (_nodes.ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id));

            var node = new Node(this, id, type, friendlyName);
            _nodes.Add(node.Id, node);
            return node;
        }

        private void StartTimer()
        {
            _timer.Interval = 60*1000;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            PublishStatsAttributesAsync();
        }

        private void OnBroadcastReceived(string topic, string newvalue)
        {
            BroadcastReceived?.Invoke(topic, newvalue);
        }


        private async Task InitializeDeviceAsync()
        {
            await PublishDeviceStateAsync();
            await PublishDeviceAttributesAsync();
            await InitializeNodesAsync();
            State = DeviceState.Ready;
            await PublishDeviceStateAsync();
            await PublishStatsAttributesAsync();
            StartTimer();
        }

        private async Task PublishDeviceAttributesAsync()
        {
            var nodes = (Nodes.Count == 0) ? "" : String.Join(",", Nodes.Select(x => x.Key));
            await Host.PublishAsync(Topic + "/" + Attributes.Homie, Constants.ProtocolVersion);
            await Host.PublishAsync(Topic + "/" + Attributes.Name, Name);
            await Host.PublishAsync(Topic + "/" + Attributes.Implementation, Constants.Implementation);
            await Host.PublishAsync(Topic + "/" + Attributes.Nodes, nodes);
            await Host.PublishAsync(Topic + "/" + Attributes.Extensions, "org.homie.legacy - stats:0.1.1:[4.x], org.homie.legacy-firmware:0.1.1:[4.x]");


            var firstMacAddress = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => nic.GetPhysicalAddress())
                    .First();

            var firstIPAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .First(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .GetIPProperties().UnicastAddresses.FirstOrDefault();

            await Host.PublishAsync(Topic + "/" + Attributes.LocalIP, (firstIPAddress!= null) ? firstIPAddress.Address.ToString() : "");
            await Host.PublishAsync(Topic + "/" + Attributes.Mac, (firstMacAddress  != null) ? PhysicalAddressToFormattedString(firstMacAddress) : "");

            await Host.PublishAsync(Topic + "/" + Attributes.FirmwareName, "sampleFW");
            await Host.PublishAsync(Topic + "/" + Attributes.FirmwareVersion, "A1.0");
        }

        String PhysicalAddressToFormattedString(PhysicalAddress address)
        {
            var builder = new StringBuilder();
            byte[] bytes = address.GetAddressBytes();
            for (int i = 0; i < bytes.Length; i++)
            {
                // Display the physical address in hexadecimal.
                builder.Append(bytes[i].ToString("X2"));
                if (i != bytes.Length - 1)
                    builder.Append(":");
                // Insert a hyphen after each byte, unless we are at the end of the
                // address.
            }
            return builder.ToString();
        }

        private async Task PublishStatsAttributesAsync()
        {
            var uptime = (int)(DateTime.Now - _startTime).TotalSeconds;
            await Host.PublishAsync(Topic + "/" + Attributes.StatsUptime, uptime.ToString());
            await Host.PublishAsync(Topic + "/" + Attributes.StatsInterval, "60");
        }

        private async Task InitializeNodesAsync()
        {
            foreach (var node in _nodes)
                await node.Value.InitializeAsync();
        }

        private async Task PublishDeviceStateAsync()
        {
            if (Host.IsConnected)
                await Host.PublishAsync(Topic + "/" + Attributes.State, StateToString());
        }

        private string StateToString()
        {
            return State.ToString().ToLower();
        }

        public void Start()
        {
            ThrowIfNoHost();
            if (_stateMachineTask != null && _stateMachineTask.Status == TaskStatus.Running)
                throw new InvalidOperationException("Device still running!");
            _cancelToken = new CancellationTokenSource();
            _stateMachineTask = CreateStateMachineTask(_cancelToken.Token);
            _stateMachineTask.Start();
            SubscribeBroadcastAsync();
            _startTime = DateTime.Now;
        }

        private async Task SubscribeBroadcastAsync()
        {
            await Host.SubscribeAsync(Constants.TopicRoot + "/" + Constants.BroadcastTopic + "/#", (topic, value) =>
            {
                OnBroadcastReceived(topic, value);
            });
        }

        public void Stop()
        {
            ThrowIfNoHost();
            StopTimer();
            UnsubscribeBroadcastAsync();
            _cancelToken.Cancel();
            _stateMachineTask?.Wait();
            State = DeviceState.Disconnected;
            PublishDeviceStateAsync();
            _stateMachineTask = null;
        }

        private async Task UnsubscribeBroadcastAsync()
        {
            await Host.UnsubscribeAsync(Constants.TopicRoot + "/" + Constants.BroadcastTopic + "/#");
        }

        private Task CreateStateMachineTask(CancellationToken cancellationToken)
        {
            return new Task(() =>
            {
                DeviceState oldState = State;
                while (!cancellationToken.IsCancellationRequested)
                {
                    switch (State)
                    {
                        case DeviceState.Init:
                            if (oldState != DeviceState.Init)
                            {
                                Host.Logger.LogInformation(" --> INIT");
                            }
                            if (TryConnect())
                            {
                                InitializeDeviceAsync().Wait(); ;
                            }
                            break;
                        case DeviceState.Ready:
                            if (oldState != DeviceState.Ready)
                            {
                                Host.Logger.LogInformation(" --> READY");
                                PublishDeviceStateAsync().Wait();
                            }
                            if (!Host.IsConnected)
                            {
                                State = DeviceState.Disconnected;
                            }
                            break;
                        case DeviceState.Disconnected:
                            if (oldState != DeviceState.Disconnected)
                            {
                                Host.Logger.LogInformation(" --> DISCONNECTED");
                            }
                            if (Host.IsConnected)
                            {
                                InitializeDeviceAsync().Wait();
                            }
                            break;
                        case DeviceState.Lost:
                            break;
                        case DeviceState.Alert:
                            break;
                        case DeviceState.Sleeping:
                            break;
                        default:
                            break;
                    }
                    oldState = State;
                    Thread.Sleep(100);
                }

            }, cancellationToken);

            bool TryConnect()
            {
                var t = Host.TryConnectAsync();
                t.Wait();
                return t.Result;
            }
        }

        private void ThrowIfNoHost()
        {
            if (Host == null)
                throw new InvalidOperationException("Connection to host must be established before calling this method");
        }
    }
}

