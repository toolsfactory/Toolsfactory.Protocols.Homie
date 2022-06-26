using System.Globalization;

namespace Toolsfactory.Protocols.Homie.Devices.Properties
{
    public class StringProperty : BaseProperty
    {
        public StringProperty(Node node, string id, string name= "default", string format = "", string unit = "", bool settable = false, bool retained = true)
            : base(node, id, DataTypes.String, name, format, unit, settable, retained) { }

        public uint MinLength { get; private set; } = 0;
        public uint MaxLength { get; private set; } = 268_435_456;

        private string _value = "";
        public string Value
        {
            get { return _value; }
            set { if (value.Length >= MinLength && value.Length <= MaxLength) { _value = value; RawValue = _value; } }
        }


        protected override string GetDefaultValue()
        {
            return "";
        }

        protected override string ValidateFormat(string? format)
        {
            if (format == null)
                return "";
            if (format.Contains(":"))
            {
                var items = format.Split(":");
                if (items.Length != 2)
                    return "";
                if (!uint.TryParse(items[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var min) ||
                    !uint.TryParse(items[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var max))
                    return "";
                MinLength = (min>=0 && min< 268_435_456) ? min : 0;
                MaxLength = (max >= MinLength && max <= 268_435_456) ? max : 268_435_456;
                return format;
            } 
            else
            {
                if(uint.TryParse(format, NumberStyles.Integer, CultureInfo.InvariantCulture, out var max))
                {
                    MaxLength = (max >= MinLength && max <= 268_435_456) ? max : 268_435_456;
                    return format;
                }
            }
            return "";
        }

        protected override bool ValidateNewValue(string value)
        {
            var result = (value.Length) >= MinLength && (value.Length <= MaxLength);
            if (result)
                _value = value;
            return result;
        }
    }

}
