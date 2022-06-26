using Toolsfactory.Protocols.Homie.Devices.Properties;

namespace Toolsfactory.Protocols.Homie.Devices
{
    public static class NodeExtensions
    {
        public static BooleanProperty AddBooleanProperty(this Node node, string id, string name = "default", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new BooleanProperty(node, id, name, unit, settable, retained);
            node.AddProperty(prop);
            return prop;
        }

        public static DateTimeProperty AddDateTimeProperty(this Node node, string id, string name = "default", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new DateTimeProperty(node, id, name, unit, settable, retained);
            node.AddProperty(prop);
            return prop;
        }

        public static EnumProperty AddEnumPropertyProperty(this Node node, string id, string format,  string name = "default", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new EnumProperty(node, id, format, name, unit, settable, retained);
            node.AddProperty(prop);
            return prop;
        }

        public static FloatProperty AddFloatProperty(this Node node, string id, string name = "default", string format = "", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new FloatProperty(node, id, name, format, unit, settable, retained);
            node.AddProperty(prop);
            return prop;
        }

        public static IntegerProperty AddIntegerProperty(this Node node, string id, string name = "default", string format = "", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new IntegerProperty(node, id, name, format, unit, settable, retained);
            node.AddProperty(prop);
            return prop;
        }

        public static PercentProperty AddPercentProperty(this Node node, string id, string name = "default", string format = "", bool settable = false, bool retained = true)
        {
            var prop = new PercentProperty(node, id, name, format, settable, retained);
            node.AddProperty(prop);
            return prop;
        }

        public static StringProperty AddStringProperty(this Node node, string id, string name = "default", string format = "", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new StringProperty(node, id, name, format, unit, settable, retained);
            node.AddProperty(prop);
            return prop;
        }

        public static Node WithBooleanProperty(this Node node, string id, string name = "default", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new BooleanProperty(node, id, name, unit, settable, retained);
            node.AddProperty(prop);
            return node;
        }

        public static Node WithDateTimeProperty(this Node node, string id, string name = "default", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new DateTimeProperty(node, id, name, unit, settable, retained);
            node.AddProperty(prop);
            return node;
        }

        public static Node WithEnumPropertyProperty(this Node node, string id, string format, string name = "default", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new EnumProperty(node, id, format, name, unit, settable, retained);
            node.AddProperty(prop);
            return node;
        }

        public static Node WithFloatProperty(this Node node, string id, string name = "default", string format = "", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new FloatProperty(node, id, name, format, unit, settable, retained);
            node.AddProperty(prop);
            return node;
        }

        public static Node WithIntegerProperty(this Node node, string id, string name = "default", string format = "", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new IntegerProperty(node, id, name, format, unit, settable, retained);
            node.AddProperty(prop);
            return node;
        }

        public static Node WithPercentProperty(this Node node, string id, string name = "default", string format = "", bool settable = false, bool retained = true)
        {
            var prop = new PercentProperty(node, id, name, format, settable, retained);
            node.AddProperty(prop);
            return node;
        }

        public static Node WithStringProperty(this Node node, string id, string name = "default", string format = "", string unit = "", bool settable = false, bool retained = true)
        {
            var prop = new StringProperty(node, id, name, format, unit, settable, retained);
            node.AddProperty(prop);
            return node;
        }



    }
}
