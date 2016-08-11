using AllJoyn.Dsb.MockDevices.Test;
using System;
using System.ComponentModel;

namespace AllJoyn.Dsb.MockDevices.Test
{
    public interface IVersionInterface
    {
        [EmitsSignalChanged(Emit = false)]
        [DocString("The interface version.")]
        [DefaultValue((ushort)1)]
        UInt16 Version { get; }
    }
}

namespace org.alljoyn.SmartSpaces.Environment
{
    [IsSecure]
    [DocString("This interface provides capability to represent current relative humidity.")]
    [InterfaceName("org.alljoyn.SmartSpaces.Environment.CurrentHumidity")]
    public interface ICurrentHumidity : IVersionInterface
    {
        [EmitsSignalChanged]
        [MinValue(0)]
        [DocString("Current relative humidity value.")]
        [DefaultValue(0d)]
        double CurrentValue { get; }

        [EmitsSignalChanged]
        [DocString("Maximum value allowed for represented relative humidity.")]
        [DefaultValue(100d)]
        double MaxValue { get; }
    }
}

namespace org.alljoyn.SmartSpaces.Operation
{
    [IsSecure]
    [DocString("This interface provides capability to switch on the device.")]
    // [InterfaceName("org.alljoyn.SmartSpaces.Operation.OnControl")]
    public interface IOnControl : IVersionInterface
    {

        [DocString("Switch on the device.")]
        void SwitchOn();
    }

    [IsSecure]
    [DocString("This interface provides capability to switch off the device.")]
    // [InterfaceName("org.alljoyn.SmartSpaces.Operation.OffControl")]
    public interface IOffControl : IVersionInterface
    {
        [DocString("Switch off the device.")]
        void SwitchOff();
    }

    [IsSecure]
    [DocString("This interface provides a capability to monitor the on/off status of device.")]
    // [InterfaceName("org.alljoyn.SmartSpaces.Operation.IOnOffStatus")]
    public interface IOnOffStatus : IVersionInterface
    {
        [EmitsSignalChanged]
        [DocString("Current on/off state of the appliance. If true, the device is on.")]
        [DefaultValue(false)]
        double OnOff { get; }
    }
}