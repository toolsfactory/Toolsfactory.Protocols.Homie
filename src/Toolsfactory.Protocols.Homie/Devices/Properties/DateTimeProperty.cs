using System;
using System.Globalization;

namespace Toolsfactory.Protocols.Homie.Devices.Properties
{
    public class DateTimeProperty : BaseProperty
    {
        public DateTimeProperty(Node node, string id, string name = "default", string  unit = "", bool settable = false, bool retained = true)
            : base(node, id, DataTypes.DateTime, name, "", unit, settable, retained) { }

        private DateTime _value = DateTime.UtcNow;
        public DateTime Value
        {
            get { return _value; }
            set { _value = value; RawValue = _value.ToString("o", System.Globalization.CultureInfo.InvariantCulture); } 
        }


        protected override string GetDefaultValue()
        {
            return DateTime.Now.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
        }

        protected override string ValidateFormat(string? format)
        {
            return "";
        }

        protected override bool ValidateNewValue(string value)
        {
            var ok = DateTime.TryParse(value, out var dt);
            if(ok)
            {
                _value = dt;
            }
            return ok;
        }
    }

}
