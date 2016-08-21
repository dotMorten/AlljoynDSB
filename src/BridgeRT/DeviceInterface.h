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
    class PropertyInterface;
    ref class DeviceInterfaceSignalListener;

    class DeviceInterface //: IAdapterSignalListener
    {
    public:
        DeviceInterface();
        virtual ~DeviceInterface();

        // IAdapterSignalListener implementation
        void AdapterSignalHandler(_In_ IAdapterSignal^ Signal);

        void Shutdown();
        QStatus Initialize(_In_ IAdapterInterface^ iface, _In_ DeviceBusObject *parent, _In_ BridgeDevice ^bridge, IAdapterSignalListener^ listener, alljoyn_busattachment ajBusAttachment);
        bool InterfaceMatchWithAdapterProperty(_In_ IAdapterProperty ^adapterProperty);
        bool InterfaceMatchWithAdapterMethod(_In_ IAdapterMethod ^adapterMethod);
        bool InterfaceMatchWithAdapterSignal(_In_ IAdapterSignal ^adapterSignal);
        bool IsMethodNameUnique(_In_ std::string name);
        bool IsSignalNameUnique(_In_ std::string name);
        void HandleSignal(IAdapterSignal ^adapterSignal);

        inline alljoyn_interfacedescription GetInterfaceDescription()
        {
            return m_interfaceDescription;
        }
        inline std::string *GetInterfaceName()
        {
            return &m_interfaceName;
        }
        inline DeviceBusObject* GetParent()
        {
            return m_parent;
        }
        bool IsAJNameUnique(_In_ std::string name);
        inline DWORD GetIndexForAJProperty()
        {
            return m_indexForAJProperty++;
        }
        inline DWORD GetIndexForMethod()
        {
            return m_indexForMethod++;
        }
        inline DWORD GetIndexForSignal()
        {
            return m_indexForSignal++;
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
        IAdapterProperty^ GetAdapterProperty()
        {
            return m_adapterProperty;
        }

        DeviceMethod * GetDeviceMethod(std::string name) 
        {
            auto index = m_deviceMethods.find(name);
            if (index == m_deviceMethods.end())
            {
                return nullptr;
            }
            return index->second;
        }

       /* std::map<std::string, DeviceSignal *> GetDeviceSignals()
        {
            return m_deviceSignals;
        }*/

        DeviceSignal * GetDeviceSignal(std::string name)
        {
            auto index = m_deviceSignals.find(name);
            if (index == m_deviceSignals.end())
            {
                return nullptr;
            }
            return index->second;
        }

    private:
        QStatus CreateDeviceProperties(IAdapterInterface^ iface, BridgeDevice ^bridge, DeviceBusObject *parent);
        QStatus CreateMethodsAndSignals(IAdapterInterface^ iface, IAdapterSignalListener^ bridge);
        QStatus GetInterfaceProperty(IAdapterProperty ^adapterProperty, PropertyInterface **propertyInterface);
		QStatus CreateAnnotations(IAdapterInterface^ iface);
        static void AJ_CALL AJMethod(_In_ alljoyn_busobject busObject, _In_ const alljoyn_interfacedescription_member* member, _In_ alljoyn_message msg);

        // alljoyn related
        alljoyn_interfacedescription m_interfaceDescription;
        std::string m_interfaceName;
        std::vector<AllJoynProperty *> m_AJProperties;
        IAdapterProperty^ m_adapterProperty;
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
        std::map<std::string, DeviceSignal *> m_deviceSignals;
        DWORD m_indexForSignal;
        DeviceInterfaceSignalListener^ m_listener;
    };
}

