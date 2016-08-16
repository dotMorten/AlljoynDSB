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

namespace BridgeRT
{
    ref class BridgeDevice;
    class PropertyInterface;
    class AllJoynProperty;

    class DeviceBusObject
    {
    public:
        DeviceBusObject();
        virtual ~DeviceBusObject();

        QStatus Initialize(_In_ IAdapterBusObject^ busObject, _In_ BridgeDevice ^parent, _In_ IAdapterSignalListener^ listener, _In_ alljoyn_busattachment attachment);
        void Shutdown();
        void EmitSignalCOV(_In_ IAdapterValue ^newValue, const std::vector<alljoyn_sessionid>& sessionIds);

        inline std::string *GetPathName()
        {
            return &m_AJBusObjectPath;
        }
        inline alljoyn_busobject GetBusObject()
        {
            return m_AJBusObject;
        }
        std::map<std::string, DeviceInterface *> GetInterfaces()
        {
            return m_interfaces;
        }
        DeviceInterface * GetInterface(std::string name)
        {
            auto index = m_interfaces.find(name);
            if (index == m_interfaces.end())
            {
                return nullptr;
            }
            return index->second;
        }
        BridgeDevice^ GetParent()
        {
            return m_parent;
        }

		alljoyn_busattachment GetBusAttachment();
        static DeviceBusObject *GetInstance(_In_ alljoyn_busobject busObject);

    private:
        // QStatus PairAjProperties();
        static QStatus AJ_CALL GetProperty(_In_ const void* context, _In_z_ const char* ifcName, _In_z_ const char* propName, _Out_ alljoyn_msgarg val);
        static QStatus AJ_CALL SetProperty(_In_ const void* context, _In_z_ const char* ifcName, _In_z_ const char* propName, _In_ alljoyn_msgarg val);

        // parent class
        BridgeDevice ^m_parent;

        // AllJoyn related
        alljoyn_busobject m_AJBusObject;
		alljoyn_busattachment m_AJBusAttachment;
        bool m_registeredOnAllJoyn;

        std::map<std::string, DeviceInterface *> m_interfaces;
        std::string m_AJBusObjectPath;
    };
}

