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

using BridgeRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AllJoyn.Dsb
{
    public sealed class SignalParameter : IAdapterValue
    {
        public object Data { get; set; }
        public string Name { get; }

        public SignalParameter(string name)
        {
            Name = name;
            Data = null;
        }
    }
    //
    // AdapterSignal.
    // Description:
    // The class that implements IAdapterSignal from BridgeRT.
    //
    public sealed class AdapterSignal : IAdapterSignal
    {
        // public properties
        public string Name { get; }

        public IList<IAdapterValue> Params { get; } = new List<IAdapterValue>();

        public AdapterSignal(string signalName)
        {
            this.Name = signalName;
        }

        internal AdapterSignal(AdapterSignal Other)
        {
            Name = Other.Name;
            Params = new List<IAdapterValue>(Other.Params);
        }
    }
}
