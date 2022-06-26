using System.Globalization;

namespace Toolsfactory.Protocols.Homie.Devices.Properties
{
    public class IntegerProperty : BaseProperty
    {
        public IntegerProperty(Node node, string id, string name = "default", string format = "", string unit = "", bool settable = false, bool retained = true)
            : base(node, id, DataTypes.Integer, name, format, unit, settable, retained) { }


        public int MinValue { get; private set; } = int.MinValue;
        public int MaxValue { get; private set; } = int.MaxValue;

        private int _value = 0;
        public int Value
        {
            get { return _value; }
            set { if (value >= MinValue && value <= MaxValue) { _value = value; RawValue = _value.ToString(CultureInfo.InvariantCulture); } }
        }

        protected override string GetDefaultValue()
        {
            return "0";
        }

        protected override string ValidateFormat(string? format)
        {
            if (format == null)
                return "";
            var items = format.Split(":");
            if (items.Length != 2)
                return "";
            if (!int.TryParse(items[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var min) || 
                !int.TryParse(items[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var max))
                return "";
            MinValue = min;
            MaxValue = max;
            return format;
        }

        protected override bool ValidateNewValue(string value)
        {
            if(int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var newval))
            {
                var result = (newval >= MinValue && newval <= MaxValue);
                if (result)
                    _value = newval;
                return result;
            }
            return false;
        }
    }

}
