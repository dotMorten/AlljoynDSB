using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BridgeRT;
using System.ComponentModel;
using AllJoyn.Dsb.MockDevices.Test;

/*
<interface name="org.alljoyn.SmartSpaces.Environment.CurrentHumidity">
    <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides capability to represent current relative humidity."/>
    <annotation name="org.alljoyn.Bus.Secure" value="true"/>
    <property name="Version" type="q" access="read">
        <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version"/>
        <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
    </property>
    <property name="CurrentValue" type="y" access="read">
        <annotation name="org.alljoyn.Bus.DocString.En" value="Current relative humidity value"/>
        <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
        <annotation name="org.alljoyn.Bus.Type.Min" value="0"/>
    </property>
    <property name="MaxValue" type="y" access="read">
        <annotation name="org.alljoyn.Bus.DocString.En" value="Maximum value allowed for represented relative humidity"/>
        <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
    </property>
</interface>
*/
namespace AllJoyn.Dsb.MockDevices
{
    public sealed class MockCurrentHumidityDevice : AdapterDevice
    {
        private IAdapterInterface _iface;
        private double _currentValue;
        private System.Threading.CancellationTokenSource _updateToken;

        public MockCurrentHumidityDevice(string name, string id, double currentHumidity) :
            base(name, "MockDevices Inc", "Mock Humidity", "1", id, "")
        {
            _iface = CreateInterface("Humidity", currentHumidity);
            BusObjects.Add(new AdapterBusObject("org.alljoyn.SmartSpaces.Environment"));
            BusObjects[0].Interfaces.Add(_iface);
            CreateEmitSignalChangedSignal();
            _currentValue = currentHumidity;
            System.Threading.Timer t = new System.Threading.Timer((o) =>
            {
                // Simulate humidity changes between 00..100
                DateTime start = (DateTime)o;
                var timeElapsed = (DateTime.Now - start).TotalMinutes;
                var newValue = Math.Sin(timeElapsed / 10) * 50 + 50;
                CurrentValue = newValue;
            }, DateTime.Now, 3000, 3000);
        }

        private static IAdapterInterface CreateInterface(string objectPath, double currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Environment.CurrentHumidity");
            iface.Annotations.Add("org.alljoyn.Bus.DocString.En", "This interface provides capability to represent current relative humidity.");
            //iface.Annotations.Add("org.alljoyn.Bus.Secure", "true");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("CurrentValue", currentValue) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Current relative humidity value");
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.Type.Min", "0");
            iface.Properties.Add(new AdapterAttribute("MaxValue", 100d) { COVBehavior = SignalBehavior.Always });
            iface.Properties[2].Annotations.Add("org.alljoyn.Bus.DocString.En", "Maximum value allowed for represented relative humidity");
            return iface;
        }

        public double CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = Math.Round(value, 1);
                //Throttle updates to 1sec so we don't saturate the bus
                if (_updateToken != null) _updateToken.Cancel();
                var token = _updateToken = new System.Threading.CancellationTokenSource();
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (!token.IsCancellationRequested)
                        UpdateValue(_currentValue);
                });
            }
        }

        public void UpdateValue(double value)
        {
            var attr = _iface.Properties.Attributes.Where(a => a.Value.Name == "CurrentValue").First();
            if (attr.Value.Data != (object)value)
            {
                attr.Value.Data = value;
                base.SignalChangeOfAttributeValue(_iface, attr);
            }
        }
    }
}
