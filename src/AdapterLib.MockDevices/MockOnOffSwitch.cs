using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeRT;
using System.ComponentModel;

namespace AdapterLib.MockDevices
{
    public sealed class MockOnOffSwitchDevice : AdapterDevice, INotifyPropertyChanged
    {
        private AdapterInterface _interfaceOn;
        private AdapterInterface _interfaceOff;
        private AdapterInterface _interfaceOnOff;
        private bool _currentValue;

        public MockOnOffSwitchDevice(string name, string id, bool isOn) :
            base(name, "MockDevices Inc", "Mock Switch", "1", id, "")
        {
            _interfaceOnOff = CreateOnOffInterface(isOn);
            _interfaceOn = CreateOnInterface(isOn);
            _interfaceOff = CreateOffInterface(!isOn);
            AdapterBusObject abo = new AdapterBusObject("org/alljoyn/SmartSpaces/Operation/Switch");
            abo.Interfaces.Add(_interfaceOnOff);
            abo.Interfaces.Add(_interfaceOn);
            abo.Interfaces.Add(_interfaceOff);
            this.BusObjects.Add(abo);
            CreateEmitSignalChangedSignal();
            _currentValue = isOn;
            // System.Threading.Timer t = new System.Threading.Timer((o) =>
            // {
            //     CurrentValue = !CurrentValue;
            // }, null, 3000, 3000);
        }

        /*
            <interface name="org.alljoyn.SmartSpaces.Operation.OnControl">
                <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides capability to switch on the device."/>
                <annotation name="org.alljoyn.Bus.Secure" value="true"/>
                <property name="Version" type="q" access="read">
                    <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
                    <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
                </property>
                <method name="SwitchOn">
                    <annotation name="org.alljoyn.Bus.DocString.En" value="Switch on the device."/>
                </method>
            </interface>
        */
        private AdapterInterface CreateOnInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnControl");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            var m = new AdapterMethod("SwitchOn", "Switch on the device.", 0, () =>
            {
                CurrentValue = true;
                System.Diagnostics.Debug.WriteLine("SwitchOn!");
                return 0;
            });
            iface.Methods.Add(m);
            return iface;
        }
        /*
        
        <interface name="org.alljoyn.SmartSpaces.Operation.OffControl">
            <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides the capability to switch off the device."/>
            <annotation name="org.alljoyn.Bus.Secure" value="true"/>
            <property name="Version" type="q" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
            </property>
            <method name="SwitchOff">
                <annotation name="org.alljoyn.Bus.DocString.En" value="Switch off the device."/>
            </method>
        </interface:
        */
        private AdapterInterface CreateOffInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OffControl");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            var m = new AdapterMethod("SwitchOff", "Switch off the device.", 0, () =>
            {
                CurrentValue = false;
                System.Diagnostics.Debug.WriteLine("SwitchOff!");
                return 0;
            });
            iface.Methods.Add(m);
            return iface;
        }

        /*
            </interface>
            <interface name="org.alljoyn.SmartSpaces.Operation.OnOffStatus">
            <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides a capability to monitor the on/off status of device."/>
            <annotation name="org.alljoyn.Bus.Secure" value="true"/>
            <property name="Version" type="q" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
            </property>
            <property name="OnOff" type="b" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="Current on/off state of the appliance. If true, the device is on state."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
            </property>
        </interface>
        */
        private static AdapterInterface CreateOnOffInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnOffStatus");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("OnOff", currentValue, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Current on/off state of the appliance. If true, the device is on state.");
            // var toggledSignal = new AdapterSignal("SwitchToggled");
            // iface.Signals.Add(toggledSignal);
            return iface;
        }

        public bool CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValue)));
                UpdateValue(_currentValue);
            }
        }

        public void UpdateValue(bool value)
        {   
            var attr = _interfaceOnOff.Properties.Where(a => a.Value.Name == "OnOff").First();
            if (attr.Value.Data != (object)value)
            {
                attr.Value.Data = value;
                SignalChangeOfAttributeValue(_interfaceOnOff, attr);
                // NotifySignalListener(Signals[0]);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
