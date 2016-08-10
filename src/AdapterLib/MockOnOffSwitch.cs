using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeRT;
using System.ComponentModel;

namespace AdapterLib
{
    public sealed class MockOnOffSwitchDevice : AdapterDevice, INotifyPropertyChanged
    {
        private Adapter _bridge;
        private IAdapterInterface _interfaceOn;
        private IAdapterInterface _interfaceOff;
        private IAdapterInterface _interfaceOnOff;
        private bool _currentValue;

        public MockOnOffSwitchDevice(Adapter bridge, string name, string id, bool isOn) :
            base(name, "MockDevices Inc", "Mock Switch", "1", id, "")
        {
            _bridge = bridge;
            _interfaceOnOff = CreateOnOffInterface(isOn);
            _interfaceOn = CreateOnInterface(isOn);
            _interfaceOff = CreateOffInterface(!isOn);
            AdapterBusObject abo = new AdapterBusObject("org/alljoyn/SmartSpaces/Operation");
            abo.Interfaces.Add(_interfaceOnOff);
            abo.Interfaces.Add(_interfaceOn);
            abo.Interfaces.Add(_interfaceOff);
            this.BusObjects.Add(abo);
            CreateEmitSignalChangedSignal();
            _currentValue = isOn;
            //System.Threading.Timer t = new System.Threading.Timer((o) =>
            //{
            //    CurrentValue = !CurrentValue;
            //}, null, 5000, 10000);
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
        private IAdapterInterface CreateOnInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnControl");
            AdapterProperty property = new AdapterProperty();
            property.Attributes.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            property.Attributes[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties = property;
            var m = new AdapterMethod("SwitchOn", "Switch on the device.", 0);
            m.InvokeAction = () =>
            {
                CurrentValue = true;
                System.Diagnostics.Debug.WriteLine("SwitchOn!");
                m.SetResult(0);
            };
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
        private IAdapterInterface CreateOffInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OffControl");
            AdapterProperty property = new AdapterProperty();
            property.Attributes.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            property.Attributes[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties = property;
            var m = new AdapterMethod("SwitchOff", "Switch off the device.", 0);
            m.InvokeAction = () =>
            {
                CurrentValue = false;
                System.Diagnostics.Debug.WriteLine("SwitchOff!");
                m.SetResult(0);
            };
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
        private static IAdapterInterface CreateOnOffInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnOffStatus");
            var property = new AdapterProperty();
            property.Attributes.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            property.Attributes[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            property.Attributes.Add(new AdapterAttribute("OnOff", currentValue, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            property.Attributes[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Current on/off state of the appliance. If true, the device is on state.");
            iface.Properties = property;
            // var toggledSignal = new AdapterSignal("SwitchToggled");
            // iface.Signals.Add(toggledSignal);
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
            var attr = _interfaceOnOff.Properties.Attributes.Where(a => a.Value.Name == "OnOff").First();
            if (attr.Value.Data != (object)value)
            {
                attr.Value.Data = value;
                _bridge.SignalChangeOfAttributeValue(this, _interfaceOnOff.Properties, attr);
                _bridge.NotifySignalListener(_interfaceOnOff.Signals[0]);
            }
            // attr = _interfaceOn.Properties.Attributes.Where(a => a.Value.Name == "IsOn").First();
            // if (attr.Value.Data != (object)value)
            // {
            //     attr.Value.Data = value;
            //     _bridge.SignalChangeOfAttributeValue(this, _interfaceOn.Properties, attr);
            // }
            // attr = _interfaceOff.Properties.Attributes.Where(a => a.Value.Name == "IsOff").First();
            // if (attr.Value.Data != (object)value)
            // {
            //     attr.Value.Data = !value;
            //     _bridge.SignalChangeOfAttributeValue(this, _interfaceOff.Properties, attr);
            // }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
