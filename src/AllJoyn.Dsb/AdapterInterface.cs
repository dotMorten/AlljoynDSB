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

        //
        // AdapterProperty.
        // Description:
        // The class that implements IAdapterProperty from BridgeRT.
        //
        private class AdapterProperty : IAdapterProperty
        {
            // public properties
            public string Name { get; }
            public IList<IAdapterAttribute> Attributes { get; }

            internal AdapterProperty()
            {
                try
                {
                    this.Attributes = new List<IAdapterAttribute>();
                }
                catch (OutOfMemoryException ex)
                {
                    throw;
                }
            }

            internal AdapterProperty(AdapterProperty Other)
            {
                try
                {
                    this.Attributes = new List<IAdapterAttribute>(Other.Attributes);
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
            }
        }
    }
}
