using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MQTTnet.Extensions.ManagedClient;

namespace Toolsfactory.Protocols.Homie.Devices.Properties
{

    public delegate void PropertyCommandReceivedDelegate(DateTime timestamp, string newvalue);

    public abstract class BaseProperty : HomieElement
    {
        public DataTypes DataType { get;  }
        public string? Format { get; }
        public bool Settable { get; }
        public bool Retained { get; }
        public string? Unit { get; }
        public string RawValue
        {
            get => _rawValue;
            set
            {
                if (ValidateNewValue(value))
                {
                    _rawValue = value;
                    PublishNewValueAsync().Wait();
                }
            }
        }
        public Node? Node { get; }

        private string? _settableTopic;
        private string _rawValue;

        public PropertyCommandReceivedDelegate? PropertyCommandReceived { get; set; }

        protected BaseProperty(Node node, string id, DataTypes dataType, string name="default", string format ="", string unit = "", bool settable = false, bool retained = true)
        : base(id, name)
        {
            DataType = dataType;
            _rawValue = "";
            Settable = false;
            Retained = true;
            Node = node;
            Topic = $"{Node.Topic}/{Id}";
            Format = ValidateFormat(format);
            Settable = settable;
            Retained = retained;
            Unit = Unit;
        }

        private void OnPropertyCommandReceived(string newvalue)
        {
            PropertyCommandReceived?.Invoke(DateTime.Now, newvalue);
        }

        public async Task ConnectAsync()
        {
            await DeleteAllSubscriptionsAsync();
            if (Settable)
                await SubscribeToSetterAsync();
            await PublishAttributesAsync();
        }

        public async Task DisconnectAsync()
        {
            await DeleteAllSubscriptionsAsync();
        }

        private async Task DeleteAllSubscriptionsAsync()
        {
            if (_settableTopic == null)
                return;
            ThrowIfNodeNotSet();
            await Node!.Device!.Host!.UnsubscribeAsync(_settableTopic);
            _settableTopic = null;

        }

        private async Task PublishAttributesAsync()
        {
            if (!Node.Device.Host.IsConnected)
                return;
            await Node.Device.Host.PublishAsync(Topic + "/$name", Name);
            await Node.Device.Host.PublishAsync(Topic + "/$datatype", DataType.ToString().ToLower());
            await Node.Device.Host.PublishAsync(Topic + "/$settable", Settable.ToString().ToLower());
            await Node.Device.Host.PublishAsync(Topic + "/$retained", Retained.ToString().ToLower());
            if (!String.IsNullOrWhiteSpace(Format))
                await Node.Device.Host.PublishAsync(Topic + "/$format", Format);
            if (!String.IsNullOrWhiteSpace(Unit))
                await Node.Device.Host.PublishAsync(Topic + "/$unit", Unit);
        }

        private async Task PublishNewValueAsync()
        {
            if (!Node.Device.Host.IsConnected)
                return;
            await Node.Device.Host.PublishAsync(Topic, RawValue, Retained);
        }


        private async Task SubscribeToSetterAsync()
        {
            _settableTopic = Topic + "/set";
            await Node.Device.Host.SubscribeAsync(_settableTopic, (topic, value) =>
            {
                if (ValidateNewValue(value))
                {
                    _rawValue = value;
                    OnPropertyCommandReceived(value);
                }
                else
                {

                }
            });
        }

        void ThrowIfNodeNotSet()
        {
            if (Node == null || Node.Device == null || Node.Device.Host == null)
                throw new InvalidOperationException("First set the Node property!");
        }


        protected abstract bool ValidateNewValue(string value);
        protected abstract string GetDefaultValue();
        protected abstract string ValidateFormat(string? format);
    }
}
