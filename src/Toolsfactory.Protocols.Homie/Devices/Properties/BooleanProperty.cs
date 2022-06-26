namespace Toolsfactory.Protocols.Homie.Devices.Properties
{
    public class BooleanProperty : BaseProperty
    {

        public BooleanProperty(Node node, string id,string name = "default", string unit = "", bool settable = false, bool retained = true) 
            : base (node, id, DataTypes.Boolean, name, "", unit, settable, retained) { }

        private bool _value = false;
        public bool Value
        {
            get { return _value; }
            set 
            { 
                _value = value;
                RawValue = value.ToString();
            }
        }


        protected override string GetDefaultValue()
        {
            return "false";
        }

        protected override string ValidateFormat(string? format)
        {
            return "";
        }

        protected override bool ValidateNewValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            value = value.ToLower();
            var result = ("true".Equals(value) || "false".Equals(value));
            if (result)
                _value = bool.Parse(value);
            return result;
        }
    }
}
