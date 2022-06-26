using System.Collections.Generic;
using System.Linq;

namespace Toolsfactory.Protocols.Homie.Devices.Properties
{
    public class EnumProperty : BaseProperty
    {
        public EnumProperty(Node node, string id, string format, string name = "default", string unit = "", bool settable = false, bool retained = true)
            : base(node, id, DataTypes.String, name, format, unit, settable, retained) { }

        public IReadOnlyList<string> AcceptedValues { get => _acceptedValues; }
        private List<string> _acceptedValues = new List<string>();

        public string Value
        {
            get { return _value; }
            set { if (ValidateNewValue(Value)) { _value = value; RawValue = value; } }
        }
        private string _value = "";


        protected override string GetDefaultValue()
        {
            return "";
        }

        protected override string ValidateFormat(string? format)
        {
            if (format == null)
                return "";
            _acceptedValues = format.Split(",").ToList();
            return format;
        }

        protected override bool ValidateNewValue(string value)
        {
            var result =_acceptedValues.Contains(value);
            if (result)
                _value = value;
            return result;
        }
    }
}
