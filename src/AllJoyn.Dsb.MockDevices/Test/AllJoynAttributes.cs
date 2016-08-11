using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllJoyn.Dsb.MockDevices.Test
{
    public abstract class AllJoynAttribute : Attribute
    {
    }
    public abstract class AllJoynAnnotationAttribute : AllJoynAttribute
    {
        string _keyName;
        protected AllJoynAnnotationAttribute(string keyName)
        {
            _keyName = keyName;
        }
        internal abstract string GetValue();
        internal string KeyName
        {
            get { return _keyName; }
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class InterfaceNameAttribute : AllJoynAttribute
    {
        public InterfaceNameAttribute(string name) : base() { Name = name; }
        public string Name { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class IsSecureAttribute : AllJoynAnnotationAttribute
    {
        public IsSecureAttribute() : base("org.alljoyn.Bus.Secure") { }
        public bool IsSecure { get; set; } = true;
        internal override string GetValue()
        {
            return IsSecure ? "true" : "false";
        }
    }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class DocStringAttribute : AllJoynAnnotationAttribute
    {
        public DocStringAttribute(string documentation, string language = "en") : base("org.alljoyn.Bus.DocString." + (language??"en")) { Language = language; Documentation = documentation; }
        public string Language { get; } = "en";
        public string Documentation { get; }
        internal override string GetValue()
        {
            return Documentation;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EmitsSignalChangedAttribute : AllJoynAnnotationAttribute
    {
        public EmitsSignalChangedAttribute() : base("org.freedesktop.DBus.Property.EmitsChangedSignal") {  }
        public bool Emit { get; set; } = true;
        internal override string GetValue()
        {
            return Emit ? "true" : "false";
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MinValueAttribute : AllJoynAnnotationAttribute
    {
        public MinValueAttribute(double minValue) : base("org.alljoyn.Bus.Type.Min") { MinValue = minValue; }
        public double MinValue { get; }
        internal override string GetValue()
        {
            return MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MaxValueAttribute : AllJoynAnnotationAttribute
    {
        public MaxValueAttribute(double maxValue) : base("org.alljoyn.Bus.Type.Max") { MaxValue = maxValue; }
        public double MaxValue { get; }
        internal override string GetValue()
        {
            return MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UnitsAttribute : AllJoynAnnotationAttribute
    {
        public UnitsAttribute() : base("org.alljoyn.Bus.Type.Units") { }        
        public string Units { get; set; }
        internal override string GetValue()
        {
            return Units;
        }
    }
}