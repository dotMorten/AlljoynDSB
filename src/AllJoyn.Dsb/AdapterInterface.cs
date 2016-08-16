using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllJoyn.Dsb
{
    public class AdapterInterface : IAdapterInterface
    {
        public AdapterInterface(string name)
        {
            Name = name;
        }

        public string Name { get; }

        IAdapterProperty IAdapterInterface.Properties { get; } = new AdapterProperty();

        public IList<IAdapterAttribute> Properties { get { return ((IAdapterInterface)this).Properties.Attributes; } }

        public IList<IAdapterMethod> Methods { get; } = new List<IAdapterMethod>();

        public IList<IAdapterSignal> Signals { get; } = new List<IAdapterSignal>();

        public IDictionary<string, string> Annotations { get; } = new Dictionary<string, string>();

        //
        // AdapterProperty.
        // Description:
        // The class that implements IAdapterProperty from BridgeRT.
        //
        private class AdapterProperty : IAdapterProperty
        {
            public IList<IAdapterAttribute> Attributes { get; }

            internal AdapterProperty()
            {
                Attributes = new List<IAdapterAttribute>();
            }

            internal AdapterProperty(AdapterProperty Other)
            {
                Attributes = new List<IAdapterAttribute>(Other.Attributes);
            }
        }
    }
}
