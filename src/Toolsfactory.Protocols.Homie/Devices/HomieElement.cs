using System;

namespace Toolsfactory.Protocols.Homie.Devices
{
    public abstract class HomieElement
    {
        public string Id { get; } = "";
        public string Name { get; } = "";
        public string Topic { get; protected set; } = "";

        protected HomieElement(string id, string name)
        {
            Id = IDVerifier.VerifyHomieID(id) ? id : throw new ArgumentException(nameof(id));
            Name = !String.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException(nameof(name));
        }
    }
}
