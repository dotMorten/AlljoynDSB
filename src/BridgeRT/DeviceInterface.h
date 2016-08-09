//
// Copyright (c) 2015, Microsoft Corporation
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR
// IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//

#pragma once

#include <vector>

namespace BridgeRT
{
    ref class BridgeDevice;
    class AllJoynProperty;
    class DeviceBusObject;
    class DeviceProperty;
    class DeviceMethod;
    class DeviceSignal;

    class DeviceInterface
    {
    public:
        DeviceInterface();
        virtual ~DeviceInterface();
        void Shutdown();
        QStatus Initialize(_In_ IAdapterInterface^ iface, _In_ DeviceBusObject *parent, _In_ BridgeDevice ^bridge);
        bool InterfaceMatchWithAdapterProperty(_In_ IAdapterProperty ^adapterProperty);
        bool InterfaceMatchWithAdapterMethod(_In_ IAdapterMethod ^adapterMethod);
        bool InterfaceMatchWithAdapterSignal(_In_ IAdapterSignal ^adapterSignal);

        inline alljoyn_interfacedescription GetInterfaceDescription()
        {
            return m_interfaceDescription;
        }
        inline std::string *GetInterfaceName()
        {
            return &m_interfaceName;
        }
        inline alljoyn_busobject GetBusObject()
        {
            return m_AJBusObject;
        }
        bool IsAJNameUnique(_In_ std::string name);
        inline DWORD GetIndexForAJProperty()
        {
            return m_indexForAJProperty++;
        }

        inline std::vector<AllJoynProperty *> &GetAJProperties()
        {
            return m_AJProperties;
        }
        // PropertyInterface *GetPropertyInterface()
        // {
        //     return m_propertyInterface;
        // }
        DeviceProperty* GetDeviceProperties()
        {
            return m_deviceProperties;
        }
    private:
        QStatus CreateDeviceProperties(IAdapterInterface^ iface, BridgeDevice ^bridge);
        QStatus CreateMethodsAndSignals(IAdapterInterface^ iface, BridgeDevice ^bridge);
        QStatus GetInterfaceProperty(IAdapterProperty ^adapterProperty, PropertyInterface **propertyInterface);

        // alljoyn related
        alljoyn_busobject m_AJBusObject;
        alljoyn_interfacedescription m_interfaceDescription;
        std::string m_interfaceName;
        std::vector<AllJoynProperty *> m_AJProperties;
        //std::vector<AllJoynSignal *> m_AJSignals;
        //std::vector<AllJoynMethod *> m_AJMethods;
        DeviceProperty * m_deviceProperties;
        std::vector<PropertyInterface *> m_propertyInterfaces;
        PropertyInterface *m_propertyInterface;

        DeviceBusObject *m_parent;
        DWORD m_indexForAJProperty;
        // list of device method
        std::map<std::string, DeviceMethod *> m_deviceMethods;
        DWORD m_indexForMethod;

        // list of Signals
        std::map<int, DeviceSignal *> m_deviceSignals;
        DWORD m_indexForSignal;
    };
}

