using System.Globalization;

namespace Toolsfactory.Protocols.Homie.Devices.Properties
{
    public class PercentProperty : BaseProperty
    {

        public PercentProperty(Node node, string id, string name = "default", string format = "", bool settable = false, bool retained = true) 
            : base(node, id, DataTypes.Percent, name, format, "%", settable, retained)
        {
        }

        public double MinValue { get; private set; } = double.MinValue;
        public double MaxValue { get; private set; } = double.MaxValue;

        private double _value = 0;
        public double Value
        {
            get { return _value; }
            set { if (value >= MinValue && value <= MaxValue) { _value = value; RawValue = _value.ToString(CultureInfo.InvariantCulture); } }
        }

        protected override string GetDefaultValue()
        {
            return "0.0";
        }

        protected override string ValidateFormat(string? format)
        {
            if (format == null)
                return "";
            var items = format.Split(":");
            if (items.Length != 2)
                return "";
            if (!double.TryParse(items[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var min) ||
                !double.TryParse(items[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var max))
                return "";
            MinValue = min;
            MaxValue = max;
            return format;
        }

        protected override bool ValidateNewValue(string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var newval))
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
