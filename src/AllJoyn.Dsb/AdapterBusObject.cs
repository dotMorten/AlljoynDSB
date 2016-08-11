using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllJoyn.Dsb
{
    public class AdapterBusObject : IAdapterBusObject
    {
        public AdapterBusObject(string objectPath)
        {
            ObjectPath = objectPath;
        }

        public string ObjectPath { get; }

        public IList<IAdapterInterface> Interfaces { get; } = new List<IAdapterInterface>();
    }
}
