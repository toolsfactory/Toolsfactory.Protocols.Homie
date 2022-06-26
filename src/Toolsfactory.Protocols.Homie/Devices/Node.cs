using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolsfactory.Protocols.Homie.Devices.Properties;

namespace Toolsfactory.Protocols.Homie.Devices
{

    public class Node : HomieElement
    {
        public Device Device { get; }
        public string Type { get; private set; } = "";
        public IReadOnlyDictionary<string, BaseProperty> Properties { get => _properties; }
        private Dictionary<string, BaseProperty> _properties = new();

        protected internal Node(Device device, string id, string @type, string friendlyName = "default")
            : base(id, friendlyName)
        {
            if (!IDVerifier.VerifyHomieID(id))
                throw new ArgumentOutOfRangeException(nameof(id));
            Device = device;
            Type = type;
            Topic = $"{Device.Topic}/{Id}";
        }

        public async Task InitializeAsync()
        {
            await PublishAttributesAsync();
            foreach (var prop in _properties)
                await prop.Value.ConnectAsync();
        }
        private async Task PublishAttributesAsync()
        {
            ThrowIfDeviceNotSet();
            var props = (Properties.Count == 0) ? "" : String.Join(",", Properties.Select(x => x.Key));
            await Device.Host!.PublishAsync(Topic + "/" + Attributes.Name, Name);
            await Device.Host!.PublishAsync(Topic + "/" + Attributes.Type, Type);
            await Device.Host!.PublishAsync(Topic + "/" + Attributes.Properties, props);
        }

        public BaseProperty AddProperty(BaseProperty prop)
        {
            // validator
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));
            if (_properties.ContainsKey(prop.Id))
                throw new ArgumentOutOfRangeException(nameof(prop.Id));
            if (prop.Node != this)
                throw new InvalidOperationException("Property linked to another Node!");
            _properties.Add(prop.Id, prop);
            return prop;
        }

        void ThrowIfDeviceNotSet()
        {
            if (Device == null)
                throw new InvalidOperationException("First set the device property!");
        }
    }
}
