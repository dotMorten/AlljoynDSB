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
#include "DsbServiceNames.h"
#include "Bridge.h"
#include "BridgeDevice.h"
#include "PropertyInterface.h"
#include "DeviceInterface.h"
#include "AllJoynProperty.h"
#include "BridgeUtils.h"

using namespace BridgeRT;
using namespace std;

static std::string EMIT_CHANGE_SIGNAL_ANNOTATION = "org.freedesktop.DBus.Property.EmitsChangedSignal";
static std::string TRUE_VALUE = "true";
static std::string CONST_VALUE = "const";
static std::string FALSE_VALUE = "false";
static std::string INVALIDATES_VALUE = "invalidates";

PropertyInterface::PropertyInterface()
    : m_interface(NULL),
    m_indexForAJProperty(1)
{
}

PropertyInterface::~PropertyInterface()
{
    for (auto ajProperty : m_AJProperties)
    {
        delete ajProperty;
    }
    m_AJProperties.clear();
}

QStatus PropertyInterface::Create(IAdapterProperty ^adapterProperty, DeviceInterface *iface)
{
    QStatus status = ER_OK;
    string tempName;
    AllJoynProperty *ajProperty = nullptr;

    // sanity check
    if (nullptr == adapterProperty)
    {
        status = ER_BAD_ARG_1;
        goto leave;
    }
    m_interface = iface;
    // m_interfaceName = ConvertTo<std::string>(iface->GetInterfaceName());
    auto m_interfaceDescription = iface->GetInterfaceDescription();

    // create alljoyn properties from attributes of adapter property
    for (auto adapterAttribute : adapterProperty->Attributes)
    {
        // create new property for AllJoyn
        ajProperty = new(std::nothrow) AllJoynProperty();
        if (nullptr == ajProperty)
        {
            status = ER_OUT_OF_MEMORY;
            goto leave;
        }

        status = ajProperty->Create(adapterAttribute, iface);
        if (ER_OK != status)
        {
            goto leave;
        }

        //property access
        uint8_t alljoynAccess;

        switch (adapterAttribute->Access)
        {
        case E_ACCESS_TYPE::ACCESS_READ: 
            alljoynAccess = ALLJOYN_PROP_ACCESS_READ;
            break;
        case E_ACCESS_TYPE::ACCESS_WRITE:
            alljoynAccess = ALLJOYN_PROP_ACCESS_WRITE;
            break;
        default:
            alljoynAccess = ALLJOYN_PROP_ACCESS_RW;
            break;
        }
        
        // expose property on alljoyn
        status = alljoyn_interfacedescription_addproperty(m_interfaceDescription, 
            ajProperty->GetName()->c_str(), 
            ajProperty->GetSignature()->c_str(), 
            alljoynAccess);
        if (ER_OK != status)
        {
            goto leave;
        }

        //add the annotations
        for (auto annotation : adapterAttribute->Annotations)
        {
            status = alljoyn_interfacedescription_addpropertyannotation(m_interfaceDescription,
                ajProperty->GetName()->c_str(),
                ConvertTo<string>(annotation->Key).c_str(),
                ConvertTo<string>(annotation->Value).c_str());
            if (ER_OK != status)
            {
                goto leave;
            }
        }
        // add change signal annotation to property if
        // the property support COV signal (Change Of Value)
        string annotationValue;
        switch (adapterAttribute->COVBehavior)
        {
        case SignalBehavior::Always:
            annotationValue = TRUE_VALUE;
            break;
        case SignalBehavior::Unspecified:
            annotationValue = FALSE_VALUE;
            break;
        case SignalBehavior::AlwaysWithNoValue:
            annotationValue = INVALIDATES_VALUE;
            break;
        case SignalBehavior::Never:
            annotationValue = CONST_VALUE;
            break;
        default:
            break;
        }
        
            status = alljoyn_interfacedescription_addpropertyannotation(m_interfaceDescription,
                ajProperty->GetName()->c_str(),
                EMIT_CHANGE_SIGNAL_ANNOTATION.c_str(),
            annotationValue.c_str());

            if (ER_OK != status)
            {
                goto leave;
            }

        // add property in list
        m_AJProperties.push_back(ajProperty);
        ajProperty = nullptr;
    }

leave:
    if (ER_OK != status &&
        nullptr != ajProperty)
    {
        delete ajProperty;
    }
    return status;
}

bool PropertyInterface::InterfaceMatchWithAdapterProperty(IAdapterProperty ^adapterProperty)
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

bool PropertyInterface::IsAJPropertyNameUnique(std::string name)
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

    return retval;
}