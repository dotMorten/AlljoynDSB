using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllJoyn.Dsb
{
    //
    // AdapterValue.
    // Description:
    // The class that implements IAdapterValue from BridgeRT.
    //
    public sealed class AdapterValue : IAdapterValue
    {
        // public properties
        public string Name { get; }
        public object Data { get; set; }

        public AdapterValue(string ObjectName, object DefaultData)
        {
            this.Name = ObjectName;
            this.Data = DefaultData;
        }

        internal AdapterValue(AdapterValue Other)
        {
            this.Name = Other.Name;
            this.Data = Other.Data;
        }
    }
}
