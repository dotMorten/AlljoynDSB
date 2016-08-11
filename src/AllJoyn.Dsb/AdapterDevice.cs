/*  
* AllJoyn Device Service Bridge for Philips Hue
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

using System;
using System.Collections.Generic;
using BridgeRT;
using System.Linq;

namespace AllJoyn.Dsb
{
    //
    // AdapterDevice.
    // Description:
    // The class that implements IAdapterDevice from BridgeRT.
    //
    public class AdapterDevice : IAdapterDevice,
                            IAdapterDeviceLightingService,
                            IAdapterDeviceControlPanel
    {
        // Object Name
        public string Name { get; }

        // Device information
        public string Vendor { get; }

        public string Model { get; }

        public string Version { get; }

        public string FirmwareVersion { get; }

        public string SerialNumber { get; }

        public string Description { get; }

        public IList<IAdapterBusObject> BusObjects { get; }

        // Device properties
        public IList<IAdapterProperty> Properties { get; }

        // Device methods
        public IList<IAdapterMethod> Methods { get; }

        // Device signals
        public IList<IAdapterSignal> Signals { get; }

        // Control Panel Handler
        public IControlPanelHandler ControlPanelHandler
        {
            get { return null; }
        }

        // Lighting Service Handler
        public ILSFHandler LightingServiceHandler
        {
            get; protected set;
        }

        // Icon
        public IAdapterIcon Icon
        {
            get; protected set;
        }
        
        protected AdapterDevice(
            string Name,
            string VendorName,
            string Model,
            string Version,
            string SerialNumber,
            string Description)
        {
            this.Name = Name;
            this.Vendor = VendorName;
            this.Model = Model;
            this.Version = Version;
            this.FirmwareVersion = Version;
            this.SerialNumber = SerialNumber;
            this.Description = Description;

            this.BusObjects = new List<IAdapterBusObject>();
            this.Properties = new List<IAdapterProperty>();
            this.Methods = new List<IAdapterMethod>();
            this.Signals = new List<IAdapterSignal>();
        }

        internal AdapterDevice(AdapterDevice Other)
        {
            this.Name = Other.Name;
            this.Vendor = Other.Vendor;
            this.Model = Other.Model;
            this.Version = Other.Version;
            this.FirmwareVersion = Other.FirmwareVersion;
            this.SerialNumber = Other.SerialNumber;
            this.Description = Other.Description;            
            this.Properties = new List<IAdapterProperty>(Other.Properties);
            this.Methods = new List<IAdapterMethod>(Other.Methods);
            this.Signals = new List<IAdapterSignal>(Other.Signals);
        }

        protected void CreateEmitSignalChangedSignal()
        {
            // change of value signal
            AdapterSignal changeOfAttributeValue = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__PROPERTY_HANDLE, null));
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__ATTRIBUTE_HANDLE, null));
            Signals.Add(changeOfAttributeValue);
        }

        internal void AddChangeOfValueSignal(
            IAdapterProperty Property,
            IAdapterValue Attribute)
        {
            try
            {
                AdapterSignal covSignal = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);

                // Property Handle
                AdapterValue propertyHandle = new AdapterValue(
                                                    Constants.COV__PROPERTY_HANDLE,
                                                    Property);

                // Attribute Handle
                AdapterValue attrHandle = new AdapterValue(
                                                    Constants.COV__ATTRIBUTE_HANDLE,
                                                    Attribute);

                covSignal.Params.Add(propertyHandle);
                covSignal.Params.Add(attrHandle);

                this.Signals.Add(covSignal);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }
        protected void SignalChangeOfAttributeValue(IAdapterInterface iface, IAdapterAttribute property)
        {
            Parent?.SignalChangeOfAttributeValue(this, iface.Properties, property);
        }
        protected void NotifySignalListener(IAdapterSignal signal)
        {
            Parent?.NotifySignalListener(signal);
        }

        internal Adapter Parent { get; set; }
    }
}
