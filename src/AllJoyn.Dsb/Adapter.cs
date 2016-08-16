/*  
* AllJoyn Device Service Bridge for Mocked lights
*  
* Copyright (c) Morten Nielsen
* All rights reserved.  
*  
* MIT License  
*  
* Permission is hereby granted, free of charge, to any person obtaining a copy of this  
* software and associated documentation files (the "Software"), to deal in the Software  
* without restriction, including without limitation the rights to use, copy, modify, merge,  
* publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons  
* to whom the Software is furnished to do so, subject to the following conditions:  
*  
* The above copyright notice and this permission notice shall be included in all copies or  
* substantial portions of the Software.  
*  
* THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,  
* INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR  
* PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE  
* FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  
* OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
* DEALINGS IN THE SOFTWARE.  
*/

using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace AllJoyn.Dsb
{

    public class BridgeConfiguration
    {
        public BridgeConfiguration(Guid deviceId, string adapterPrefix)
        {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));
            DeviceID = deviceId;
            if (string.IsNullOrWhiteSpace(adapterPrefix) || adapterPrefix.Contains(" "))
                throw new ArgumentException(nameof(adapterPrefix));
            AdapterPrefix = adapterPrefix;
        }
        public Guid DeviceID { get; }
        // the adapter prefix must be something like "com.mycompany" (only alpha num and dots)
        // it is used by the Device System Bridge as root string for all services and interfaces it exposes
        public string AdapterPrefix { get; }
        public string Vendor { get; set; }
        public string ApplicationName { get; set; }
        public string DeviceName { get; set; }
        public string ModelName { get; set; }
    }
    public class Adapter : IAdapter
    {
        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_INVALID_HANDLE = 6;

        // Device Arrival and Device Removal Signal Indices
        private const int DEVICE_ARRIVAL_SIGNAL_INDEX = 0;
        private const int DEVICE_ARRIVAL_SIGNAL_PARAM_INDEX = 0;
        private const int DEVICE_REMOVAL_SIGNAL_INDEX = 1;
        private const int DEVICE_REMOVAL_SIGNAL_PARAM_INDEX = 0;

        public string Vendor { get; } = "";

        public string AdapterName { get; } = "";

        public string Version { get; } = "";

        public string ExposedAdapterPrefix { get; } = "";

        public string ExposedApplicationName { get; } = "";

        public string ExposedDeviceName { get; } = "";

        public Guid ExposedApplicationGuid { get; }

        public IList<IAdapterSignal> Signals { get; }

        public IList<IAdapterBusObject> BusObjects { get; } = new List<IAdapterBusObject>();

        public Adapter(BridgeConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
            Windows.ApplicationModel.PackageId packageId = package?.Id;
            
            this.Vendor = configuration.Vendor;
            this.ExposedApplicationName = configuration.ApplicationName;
            this.ExposedDeviceName = configuration.DeviceName ?? "";
            this.AdapterName = configuration.ModelName ?? "";
            // the adapter prefix must be something like "com.mycompany" (only alpha num and dots)
            // it is used by the Device System Bridge as root string for all services and interfaces it exposes
            this.ExposedAdapterPrefix = configuration.AdapterPrefix;
            this.ExposedApplicationGuid = configuration.DeviceID;

            if (null != package && null != packageId)
            {
                if(string.IsNullOrWhiteSpace(AdapterName))
                    AdapterName = packageId.Name;
                Windows.ApplicationModel.PackageVersion versionFromPkg = packageId.Version;
                this.Version = $"{versionFromPkg.Major}.{versionFromPkg.Minor}.{versionFromPkg.Revision}.{versionFromPkg.Build}";
                if (string.IsNullOrWhiteSpace(Vendor))
                    Vendor = package.PublisherDisplayName;
                if (string.IsNullOrWhiteSpace(ExposedApplicationName))
                    ExposedApplicationName = package.DisplayName;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ExposedApplicationName))
                    this.ExposedApplicationName = "DSB";
                this.Version = "0.0.0.0";
            }

            try
            {
                this.Signals = new List<IAdapterSignal>();
                this.devices = new List<IAdapterDevice>();
                this.signalListeners = new Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>>();

                //var EnableJoinMethod = new AdapterMethod("Find Hue Bridges", "Searches for new hue bridges", 0);
                //EnableJoinMethod.InvokeAction = LoadBridges;

                //Create Adapter Signals
                this.createSignals();
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
        }

        public void AddDevice(IAdapterDevice device)
        {
            if (device is AdapterDevice)
            {
                if (((AdapterDevice)device).Parent != null)
                    throw new InvalidOperationException("Device has already been added to an adapter");
                ((AdapterDevice)device).Parent = this;
            }
            this.devices.Add(device);
            this.NotifyDeviceArrival(device);
        }

        public void RemoveDevice(IAdapterDevice device)
        {
            this.devices.Remove(device);
            if (device is AdapterDevice)
            {
                ((AdapterDevice)device).Parent = null;
            }
            this.NotifyDeviceRemoval(device);
        }

        public uint SetConfiguration([ReadOnlyArray] byte[] ConfigurationData)
        {
            return ERROR_SUCCESS;
        }

        public uint GetConfiguration(out byte[] ConfigurationDataPtr)
        {
            ConfigurationDataPtr = null;

            return ERROR_SUCCESS;
        }

        public uint Initialize()
        {
            return ERROR_SUCCESS;
        }

        public uint Shutdown()
        {
            return ERROR_SUCCESS;
        }

        public uint EnumDevices(
            ENUM_DEVICES_OPTIONS Options,
            out IList<IAdapterDevice> DeviceListPtr,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;
            DeviceListPtr = new List<IAdapterDevice>(this.devices);
            return ERROR_SUCCESS;
        }

        public uint GetProperty(
            IAdapterProperty Property,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        public uint SetProperty(
            IAdapterProperty Property,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        public uint GetPropertyValue(
            IAdapterProperty Property,
            string AttributeName,
            out IAdapterValue ValuePtr,
            out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            // find corresponding attribute
            foreach (var attribute in ((IAdapterProperty)Property).Attributes)
            {
                if (attribute.Value.Name == AttributeName)
                {
                    ValuePtr = attribute.Value;
                    return ERROR_SUCCESS;
                }
            }

            return ERROR_INVALID_HANDLE;
        }

        public uint SetPropertyValue(
            IAdapterProperty Property,
            IAdapterValue Value,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            // find corresponding attribute
            foreach (var attribute in ((IAdapterProperty)Property).Attributes)
            {
                if (attribute.Value.Name == Value.Name)
                {
                    try
                    {
                        var result = attribute.OnValueSet(Value.Data);
                        if (result == (uint)AllJoynStatusCode.Ok)
                        {
                            attribute.Value.Data = Value.Data;
                        }
                        return result;
                    }
                    catch
                    {
                        return (uint)AllJoynStatusCode.OsError;
                    }
                }
            }

            return ERROR_INVALID_HANDLE;
        }

        public uint CallMethod(
            IAdapterMethod Method,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;
            if(Method is AdapterMethod)
            {
                ((AdapterMethod)Method).Invoke();
            }
            return ERROR_SUCCESS;
        }

        public uint RegisterSignalListener(
            IAdapterSignal Signal,
            IAdapterSignalListener Listener,
            object ListenerContext)
        {
            if (Signal == null || Listener == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            int signalHashCode = Signal.GetHashCode();

            SIGNAL_LISTENER_ENTRY newEntry;
            newEntry.Signal = Signal;
            newEntry.Listener = Listener;
            newEntry.Context = ListenerContext;

            lock (this.signalListeners)
            {
                if (this.signalListeners.ContainsKey(signalHashCode))
                {
                    this.signalListeners[signalHashCode].Add(newEntry);
                }
                else
                {
                    IList<SIGNAL_LISTENER_ENTRY> newEntryList = new List<SIGNAL_LISTENER_ENTRY>();
                    newEntryList.Add(newEntry);
                    this.signalListeners.Add(signalHashCode, newEntryList);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint UnregisterSignalListener(
            IAdapterSignal Signal,
            IAdapterSignalListener Listener)
        {
            return ERROR_SUCCESS;
        }

        public uint NotifySignalListener(IAdapterSignal Signal)
        {
            if (Signal == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            int signalHashCode = Signal.GetHashCode();

            lock (this.signalListeners)
            {
                IList<SIGNAL_LISTENER_ENTRY> listenerList = this.signalListeners[signalHashCode];
                foreach (SIGNAL_LISTENER_ENTRY entry in listenerList)
                {
                    IAdapterSignalListener listener = entry.Listener;
                    object listenerContext = entry.Context;
                    listener.AdapterSignalHandler(Signal, listenerContext);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint NotifyDeviceArrival(IAdapterDevice Device)
        {
            if (Device == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            IAdapterSignal deviceArrivalSignal = this.Signals[DEVICE_ARRIVAL_SIGNAL_INDEX];
            IAdapterValue signalParam = deviceArrivalSignal.Params[DEVICE_ARRIVAL_SIGNAL_PARAM_INDEX];
            signalParam.Data = Device;
            this.NotifySignalListener(deviceArrivalSignal);

            return ERROR_SUCCESS;
        }

        public uint NotifyDeviceRemoval(IAdapterDevice Device)
        {
            if (Device == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            IAdapterSignal deviceRemovalSignal = this.Signals[DEVICE_REMOVAL_SIGNAL_INDEX];
            IAdapterValue signalParam = deviceRemovalSignal.Params[DEVICE_REMOVAL_SIGNAL_PARAM_INDEX];
            signalParam.Data = Device;
            this.NotifySignalListener(deviceRemovalSignal);

            return ERROR_SUCCESS;
        }

        private void createSignals()
        {
            // Device Arrival Signal
            AdapterSignal deviceArrivalSignal = new AdapterSignal(Constants.DEVICE_ARRIVAL_SIGNAL);
            AdapterValue deviceHandle_arrival = new AdapterValue(
                                                        Constants.DEVICE_ARRIVAL__DEVICE_HANDLE,
                                                        null);
            deviceArrivalSignal.Params.Add(deviceHandle_arrival);

            // Device Removal Signal
            AdapterSignal deviceRemovalSignal = new AdapterSignal(Constants.DEVICE_REMOVAL_SIGNAL);
            AdapterValue deviceHandle_removal = new AdapterValue(
                                                        Constants.DEVICE_REMOVAL__DEVICE_HANDLE,
                                                        null);
            deviceRemovalSignal.Params.Add(deviceHandle_removal);

            // Add Signals to the Adapter Signals
            this.Signals.Add(deviceArrivalSignal);
            this.Signals.Add(deviceRemovalSignal);
        }

        internal void SignalChangeOfAttributeValue(IAdapterDevice device, IAdapterProperty property, IAdapterAttribute attribute)
        {
            // find change of value signal of that end point (end point == bridgeRT device)

            var covSignal = device.Signals.OfType<AdapterSignal>().FirstOrDefault(s => s.Name == Constants.CHANGE_OF_VALUE_SIGNAL);
            if (covSignal == null)
            {
                // no change of value signal
                return;
            }

            // set property and attribute param of COV signal
            // note that 
            // - ZCL cluster correspond to BridgeRT property 
            // - ZCL attribute correspond to BridgeRT attribute 
            var param = covSignal.Params.FirstOrDefault(p => p.Name == Constants.COV__PROPERTY_HANDLE);
            if (param == null)
            {
                // signal doesn't have the expected parameter
                return;
            }
            param.Data = property;

            param = covSignal.Params.FirstOrDefault(p => p.Name == Constants.COV__ATTRIBUTE_HANDLE);
            if (param == null)
            {
                // signal doesn't have the expected parameter
                return;
            }
            param.Data = attribute.Value;

            // signal change of value to BridgeRT
            NotifySignalListener(covSignal);
        }

        private struct SIGNAL_LISTENER_ENTRY
        {
            // The signal object
            internal IAdapterSignal Signal;

            // The listener object
            internal IAdapterSignalListener Listener;

            //
            // The listener context that will be
            // passed to the signal handler
            //
            internal object Context;
        }

        // List of Devices
        private IList<IAdapterDevice> devices;

        // A map of signal handle (object's hash code) and related listener entry
        private Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>> signalListeners;
    }
}
