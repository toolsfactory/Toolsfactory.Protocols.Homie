using System;
using System.Threading.Tasks;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using MQTTnet.Client.Options;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Toolsfactory.Protocols.Homie.Devices
{
    public class HomieDeviceHost : IDeviceHost, IConnectingFailedHandler
    {
        private readonly Device _device;
        private readonly HomieHostConfiguration _config;
        private readonly IManagedMqttClient _mqttClient;
        private readonly Dictionary<string, Action<string, string>> _subscriptions = new();
        private readonly Dictionary<string, Action<string, string>> _subscriptionsWithWildcard = new();


        public HomieDeviceHost(Device device, HomieHostConfiguration config, ILogger<HomieDeviceHost> logger)
        {
            _device = device;
            _config = config;
            Logger = logger;
            _mqttClient = new MqttFactory().CreateManagedMqttClient();
            _mqttClient.ConnectingFailedHandler = this;
            _device.Host = this;
        }

        public ILogger<IDeviceHost> Logger { get; }
        public bool IsConnected { get { return _mqttClient.IsConnected; } }
        public bool IsStarted { get { return _mqttClient.IsStarted; } }
        public Task StartAsync()
        {
            return Task.Run(() => {
                if (IsStarted)
                    return;

                var opts = CreateMqttClientOptions();
                _mqttClient.UseApplicationMessageReceivedHandler(MqttMessageReceiver);
                _mqttClient.StartAsync(opts).Wait();
                _device.Start();
            });
        }

        private void MqttMessageReceiver(MqttApplicationMessageReceivedEventArgs args)
        {
            Logger.LogInformation($"Message received - Topic: {args.ApplicationMessage.Topic} - Value: {args.ApplicationMessage.ConvertPayloadToString()}");
            if(_subscriptions.TryGetValue(args.ApplicationMessage.Topic, out var action))
            {
                action(args.ApplicationMessage.Topic ,args.ApplicationMessage.ConvertPayloadToString());
            }
            foreach(var sub in _subscriptionsWithWildcard)
            {
                if (MqttHelpers.IsMatch(args.ApplicationMessage.Topic, sub.Key))
                {
                    sub.Value(args.ApplicationMessage.Topic, args.ApplicationMessage.ConvertPayloadToString());
                }
            }
        }

        private IManagedMqttClientOptions CreateMqttClientOptions()
        {
            var lastwill = new MqttApplicationMessageBuilder()
                .WithTopic(_device.Topic + Constants.LevelSeparator + Attributes.State)
                .WithPayload(DeviceState.Lost.ToString().ToLower())
                .Build();

            var clientopts = new MqttClientOptionsBuilder()
                .WithClientId(_device.Id + "_client")
                .WithTcpServer(_config.TcpServer, _config.Port)
                .WithWillMessage(lastwill);

            if (!String.IsNullOrWhiteSpace(_config.Username))
                clientopts.WithCredentials(_config.Username, _config.Password);

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(_config.ReconnectDelay))
                .WithClientOptions(clientopts.Build());

            return options.Build();
        }

        public async Task StopAsync()
        {
            if (!IsStarted)
                return;
            _device.Stop();
            await _mqttClient.StopAsync();
        }

        public Task<bool> TryConnectAsync()
        {
            return Task.FromResult(IsConnected);
        }

        public async Task SubscribeAsync(string topic, Action<string, string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (MqttHelpers.HasWildcard(topic))
            {
                if (_subscriptionsWithWildcard.ContainsKey(topic))
                    throw new ArgumentException("topic already subscribed!");
                _subscriptionsWithWildcard.Add(topic, action);
            }
            else
            {
                if (_subscriptions.ContainsKey(topic))
                    throw new ArgumentException("topic already subscribed!");
                _subscriptions.Add(topic, action);
            }
            await _mqttClient.SubscribeAsync(topic);
        }
        public async Task UnsubscribeAsync(string topic)
        {
            await _mqttClient.UnsubscribeAsync(topic);
            if (_subscriptions.ContainsKey(topic))
                _subscriptions.Remove(topic);
            if (_subscriptionsWithWildcard.ContainsKey(topic))
                _subscriptionsWithWildcard.Remove(topic);
        }

        public async Task PublishAsync(string topic, string payload, bool retained = true)
        {
            await _mqttClient.PublishAsync(topic, payload, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, retained);
        }

        public Task HandleConnectingFailedAsync(ManagedProcessFailedEventArgs eventArgs)
        {
            Logger.XLogConnectingFailed(eventArgs.Exception);
            return Task.CompletedTask;
        }
    }

}

