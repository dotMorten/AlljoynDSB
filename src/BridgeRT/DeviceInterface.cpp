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

#include "pch.h"
#include <sstream>

#include "DsbServiceNames.h"
#include "Bridge.h"
#include "BridgeDevice.h"
#include "DeviceProperty.h"
#include "DeviceMethod.h"
#include "DeviceSignal.h"
#include "PropertyInterface.h"
#include "DeviceInterface.h"
#include "DeviceBusObject.h"
#include "AllJoynProperty.h"
#include "BridgeUtils.h"

using namespace BridgeRT;
using namespace std;

static std::string EMIT_CHANGE_SIGNAL_ANNOTATION = "org.freedesktop.DBus.Property.EmitsChangedSignal";
static std::string TRUE_VALUE = "true";
static std::string CONST_VALUE = "const";
static std::string FALSE_VALUE = "false";
static std::string INVALIDATES_VALUE = "invalidates";

DeviceInterface::DeviceInterface()
    : m_interfaceDescription(NULL),
    m_indexForAJProperty(1)
{
}

DeviceInterface::~DeviceInterface()
{
    for (auto ajProperty : m_AJProperties)
    {
        delete ajProperty;
    }
    m_AJProperties.clear();
}

QStatus DeviceInterface::Initialize(IAdapterInterface^ iface, DeviceBusObject *parent, BridgeDevice ^bridge)
{
    QStatus status = ER_OK;
    string tempName;
    AllJoynProperty *ajProperty = nullptr;

    // sanity check
    if (nullptr == iface)
    {
        status = ER_BAD_ARG_1;
        goto leave;
    }
    if (0 == iface->Name->Length())
    {
        status = ER_BAD_ARG_1;
        goto leave;
    }
    if (nullptr == parent)
    {
        status = ER_BAD_ARG_2;
        goto leave;
    }

    m_interfaceName = ConvertTo<std::string>(iface->Name);

    // create alljoyn interface 
    // note that the interface isn't suppose to already exist => ER_BUS_IFACE_ALREADY_EXISTS is an error
    if (DsbBridge::SingleInstance()->GetConfigManager()->IsDeviceAccessSecured())
    {
        status = alljoyn_busattachment_createinterface_secure(bridge->GetBusAttachment(), m_interfaceName.c_str(), &m_interfaceDescription, AJ_IFC_SECURITY_REQUIRED);
    }
    else
    {
        status = alljoyn_busattachment_createinterface(bridge->GetBusAttachment(), m_interfaceName.c_str(), &m_interfaceDescription);
    }
    if (ER_OK != status)
    {
        return status;
    }

    status = CreateDeviceProperties(iface, bridge);
    if (ER_OK != status)
    {
        return status;
    }
    status = CreateMethodsAndSignals(iface, bridge);
    if (ER_OK != status)
    {
        return status;
    }

    alljoyn_interfacedescription_activate(m_interfaceDescription);
    status = alljoyn_busobject_addinterface_announced(parent->GetBusObject(), m_interfaceDescription);

leave:
    if (ER_OK != status &&
        nullptr != ajProperty)
    {
        delete ajProperty;
    }
    return status;
}

void DeviceInterface::Shutdown()
{
    m_deviceProperties->Shutdown();
    delete m_deviceProperties;
    delete m_propertyInterface;

    for (auto val : m_deviceMethods)
    {
        delete val.second;
    }
    m_deviceMethods.clear();

    for (auto val : m_deviceSignals)
    {
        delete val.second;
    }
    m_deviceSignals.clear();
}

QStatus DeviceInterface::CreateDeviceProperties(IAdapterInterface^ iface, BridgeDevice ^bridge)
{
    QStatus status = ER_OK;

    PropertyInterface *newInterface = nullptr;

    m_propertyInterface = nullptr;

    // create new interface
    newInterface = new(std::nothrow) PropertyInterface();
    if (nullptr == newInterface)
    {
        status = ER_OUT_OF_MEMORY;
        goto leave;
    }

    status = newInterface->Create(iface->Properties, this, bridge);
    if (ER_OK != status)
    {
        goto leave;
    }
    m_propertyInterface = newInterface;

    DeviceProperty *deviceProperty = nullptr;
    // create device property
    deviceProperty = new (std::nothrow) DeviceProperty();
    if (nullptr == deviceProperty)
    {
        status = ER_OUT_OF_MEMORY;
        goto leave;
    }


    status = deviceProperty->Initialize(iface->Properties, m_propertyInterface, bridge);
    if (ER_OK != status)
    {
        goto leave;
    }
    m_deviceProperties = deviceProperty;

    deviceProperty = nullptr;

leave:
    /*if (ER_OK != status &&
        nullptr != deviceProperty)
    {
        delete deviceProperty;
    }*/
    return status;
}

QStatus DeviceInterface::CreateMethodsAndSignals(IAdapterInterface^ iface, BridgeDevice ^bridge)
{
    QStatus status = ER_OK;
    DeviceMethod *method = nullptr;
    DeviceSignal *signal = nullptr;

    if (nullptr != iface->Methods)
    {
        for (auto adapterMethod : iface->Methods)
        {
            // method = new(std::nothrow) DeviceMethod();
            // if (nullptr == method)
            // {
            //     status = ER_OUT_OF_MEMORY;
            //     return status;
            // }
            // 
            // status = method->Initialize(this, adapterMethod);
            // if (ER_OK != status)
            // {
            //     return status;
            // }
            // 
            // m_deviceMethods.insert(std::make_pair(method->GetName(), method));
            // method = nullptr;
        }
    }

    // create signals
    if (nullptr != iface->Signals)
    {
        for (auto adapterSignal : iface->Signals)
        {
            // if (adapterSignal->Name == Constants::CHANGE_OF_VALUE_SIGNAL)
            // {
            //     // change of value signal only concerns IAdapterProperty hence not this class
            //     continue;
            // }
            // 
            // signal = new(std::nothrow) DeviceSignal();
            // if (nullptr == signal)
            // {
            //     return ER_OUT_OF_MEMORY;
            // }
            // 
            // status = signal->Initialize(this, adapterSignal);
            // if (ER_OK != status)
            // {
            //     return status;
            // }
            // 
            // m_deviceSignals.insert(std::make_pair(adapterSignal->GetHashCode(), signal));
            // signal = nullptr;
        }
    }

    return status;
}

bool DeviceInterface::InterfaceMatchWithAdapterMethod(IAdapterMethod ^adapterMethod)
{
    return false; // TODO
}

bool DeviceInterface::InterfaceMatchWithAdapterSignal(IAdapterSignal ^adapterSignal)
{
    return false; // TODO
}

bool DeviceInterface::InterfaceMatchWithAdapterProperty(IAdapterProperty ^adapterProperty)
{
    bool retVal = false;
    vector <IAdapterAttribute ^> tempList;

    // create temporary list of IAdapterValue that have to match with one of the 
    // AllJoyn properties
    for (auto adapterAttr : adapterProperty->Attributes)
    {
        tempList.push_back(adapterAttr);
    }

    // go through AllJoyn properties and find matching IAdapterValue
    for (auto ajProperty : m_AJProperties)
    {
        retVal = false;
        auto adapterAttr = tempList.end();
        for (adapterAttr = tempList.begin(); adapterAttr != tempList.end(); adapterAttr++)
        {
            if (ajProperty->IsSameType(*adapterAttr))
            {
                retVal = true;
                break;
            }
        }
        if (retVal)
        {
            // remove adapterValue from temp list
            tempList.erase(adapterAttr);
        }
        else
        {
            // interface doesn't match
            break;
        }
    }

    return retVal;
}

bool DeviceInterface::IsAJNameUnique(std::string name)
{
    bool retval = true;

    for (auto ajProperty : m_AJProperties)
    {
        if (name == *ajProperty->GetName())
        {
            retval = false;
            break;
        }
    }
    // TODO: Check signals and methods too
    return retval;
}
