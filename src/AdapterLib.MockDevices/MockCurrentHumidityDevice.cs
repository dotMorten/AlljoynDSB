using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeRT;
using System.ComponentModel;

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
namespace AdapterLib.MockDevices
{
    public sealed class MockCurrentHumidityDevice : AdapterDevice, INotifyPropertyChanged
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
            CurrentValue = currentHumidity;
        }
        private static IAdapterInterface CreateInterface(string objectPath, double currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Environment.CurrentHumidity");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("CurrentValue", currentValue, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Current relative humidity value");
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.Type.Min", "0");
            iface.Properties.Add(new AdapterAttribute("MaxValue", 100d, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            iface.Properties[2].Annotations.Add("org.alljoyn.Bus.DocString.En", "Maximum value allowed for represented relative humidity");
            return iface;
        }

        private void CreateEmitSignalChangedSignal()
        {
            // change of value signal
            AdapterSignal changeOfAttributeValue = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__PROPERTY_HANDLE, null));
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__ATTRIBUTE_HANDLE, null));
            Signals.Add(changeOfAttributeValue);
        }

        public double CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = Math.Round(value, 1);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValue)));

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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
